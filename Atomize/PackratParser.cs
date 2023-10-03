using Atomize;
using System.Data.SqlTypes;
using System.Runtime.Serialization;

namespace Atomize;

internal class PackratParser<T>
{
   private readonly IDictionary<long, IDictionary<int, IParseResult<T>>> _parsed;
   private readonly Parser<T> _parser;

   public PackratParser(Parser<T> parser)
   {
      _parsed = new Dictionary<long, IDictionary<int, IParseResult<T>>>();
      _parser = parser;
   }

   public IParseResult<T> Apply(TextScanner scanner)
   {
      if (scanner.PackratIdentifier == 0)
         scanner.PackratIdentifier = GlobalIdentifier.GetId(scanner);

      var at = scanner.Offset;

      if (!_parsed.TryGetValue(scanner.PackratIdentifier, out var results))
      {
         var result = _parser(scanner);
         results = new Dictionary<int, IParseResult<T>>() { [at] = result };
         _parsed[scanner.PackratIdentifier] = results;

         return result;
      }

      if (results.TryGetValue(at, out var parsedResult))
      {
         scanner.Advance(parsedResult.Length);

         return parsedResult;
      }

      parsedResult = _parser(scanner);

      results[at] = parsedResult;

      return parsedResult;
   }
}

internal class DLRParser<T>
{
   private readonly IDictionary<long, IDictionary<int, IParseResult<T>>> _parsed;
   private readonly Parser<T> _parser;

   public DLRParser(Parser<T> parser)
   {
      _parsed = new Dictionary<long, IDictionary<int, IParseResult<T>>>();
      _parser = parser;
   }

   public IParseResult<T> Apply(TextScanner scanner)
   {
      if (scanner.PackratIdentifier == 0)
         scanner.PackratIdentifier = GlobalIdentifier.GetId(scanner);

      var at = scanner.Offset;

      if (!_parsed.TryGetValue(scanner.PackratIdentifier, out var results))
      {
         results = new Dictionary<int, IParseResult<T>>();

         _parsed[scanner.PackratIdentifier] = results;
      }

      if (!results.TryGetValue(at, out var result))
      {
         var lr = new LR(at);

         results[at] = lr;

         result = _parser(scanner);

         results[at] = result;

         if (lr.Detected && result.IsMatch)
            return GrowSeed(scanner, at, ref results);

         return result;
      }

      scanner.Offset = result.Offset + result.Length;

      if (result is LR leftRecursive)
      {
         leftRecursive.Detected = true;

         return Parse.Fail<T>(scanner);
      }

      return result;
   }

   private IParseResult<T> GrowSeed(TextScanner scanner, int at, ref IDictionary<int, IParseResult<T>> results)
   {
      var seed = results[at];

      while (true)
      {
         scanner.Offset = at;

         var result = _parser(scanner);
         var next = seed.Offset + seed.Length;

         if (!result.IsMatch || scanner.Offset <= next)
            break;

         seed = results[at] = result;
      }

      scanner.Offset = seed.Offset + seed.Length;

      return seed;
   }

   private class LR : IParseResult<T>
   {
      public LR(int at)
      {
         Offset = at;
         Detected = false;
      }

      public bool Detected { get; set; }

      public bool IsMatch => false;

      public int Length => 0;

      public int Offset { get; }

      public T? Value => default;

      public string Why => "";
   }
}

internal class ILRParser<T>
{
   private readonly long __id;
   private readonly IDictionary<int, LRHead> _heads;
   private readonly IDictionary<long, IDictionary<int, IParseResult<T>>> _parsed;
   private readonly Parser<T> _parser;

   private LR? _lrstack;

   public ILRParser(Parser<T> parser)
   {
      __id = GlobalIdentifier.GetId(parser);
      _heads = new Dictionary<int, LRHead>();
      _parsed = new Dictionary<long, IDictionary<int, IParseResult<T>>>();
      _parser = parser;
   }

   public IParseResult<T> Apply(TextScanner scanner)
   {
      var results = SetupPackrat(scanner);
      var at = scanner.Offset;
      var recall = Recall(scanner, at);

      if (recall is null)
      {
         var lr = new LR(at, Parse.Fail<T>(scanner), _parser, _lrstack);

         results[at] = recall = _lrstack = lr;

         var result = _parser(scanner);

         _lrstack = _lrstack.Next;

         if (lr.Head is not null)
         {
            lr.Seed = result;

            return LRAnswer(scanner, at, lr, in results);
         }

         results[at] = result;

         return result;
      }

      scanner.Offset = recall.Offset + recall.Length;

      if (recall is LR ilr)
      {
         SetupLeftRecursion(ilr);

         results[at] = ilr.Seed;

         return ilr.Seed;
      }

      return recall;
   }

   private IParseResult<T> GrowSeed(TextScanner scanner, int at, in IDictionary<int, IParseResult<T>> results, LRHead head)
   {
      var seed = results[at];

      _heads[at] = head;

      while (true)
      {
         scanner.Offset = at;
         head.EvalSet = new HashSet<long>(head.InvolvedSet);
         var result = _parser(scanner);
         var next = seed.Offset + seed.Length;

         if (!result.IsMatch || scanner.Offset <= next)
            break;

         seed = results[at] = result;
      }

      _heads.Remove(at);

      scanner.Offset = seed.Offset + seed.Length;

      return seed;
   }

   private IParseResult<T> LRAnswer(TextScanner scanner, int at, LR lr, in IDictionary<int, IParseResult<T>> results)
   {
      var head = lr.Head;

      if (head is null || head.Parser != _parser)
         return lr.Seed;

      results[at] = lr.Seed;

      if (!lr.Seed.IsMatch)
         return lr.Seed;

      return GrowSeed(scanner, at, results, head);
   }

   private IParseResult<T>? Recall(TextScanner scanner, int at)
   {
      var results = _parsed[scanner.PackratIdentifier];
      var hasResult = results.TryGetValue(at, out var result);

      if (!_heads.TryGetValue(at, out var head))
         return result;

      if (!hasResult && (_parser != head.Parser) && !head.InvolvedSet.Contains(__id))
         return Parse.Fail<T>(scanner);

      if (head.EvalSet.Contains(__id))
      {
         head.EvalSet.Remove(__id);
         result = _parser(scanner);
      }

      return result;
   }

   private void SetupLeftRecursion(LR lr)
   {
      lr.Head ??= new(_parser);

      var stack = _lrstack;

      while (stack is not null && stack.Head != lr.Head)
      {
         stack.Head = lr.Head;
         lr.Head.InvolvedSet.Add(GlobalIdentifier.GetId(stack.Parser));
         stack = stack.Next;
      }
   }

   private IDictionary<int, IParseResult<T>> SetupPackrat(TextScanner scanner)
   {
      if (!_parsed.TryGetValue(scanner.PackratIdentifier, out var results))
      {
         results = new Dictionary<int, IParseResult<T>>();

         _parsed[scanner.PackratIdentifier] = results;
      }

      return results;
   }

   private class LR : IParseResult<T>
   {
      public LR(int at, IParseResult<T> seed, Parser<T> parser, LR? next = null)
      {
         Offset = at;
         Head = null;
         Seed = seed;
         Next = next;
         Parser = parser;
      }

      public bool IsMatch => true;

      public int Length => 0;

      public int Offset { get; }

      public T? Value => default;

      public string Why => "";

      public LRHead? Head { get; set; }

      public LR? Next { get; }

      public Parser<T> Parser { get; }

      public IParseResult<T> Seed { get; set; }

   }

   private class LRHead
   {
      public LRHead(Parser<T> parser)
      {
         EvalSet = new HashSet<long>();
         InvolvedSet = new HashSet<long>();
         Parser = parser;
      }

      public ISet<long> EvalSet { get; set; }

      public ISet<long> InvolvedSet { get; }

      public Parser<T> Parser { get; }
   }

   
}