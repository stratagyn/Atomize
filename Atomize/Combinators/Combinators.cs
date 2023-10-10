using System.Text.RegularExpressions;
using static Atomize.Failure;

namespace Atomize;

public static partial class Parse
{
   public static Parser<char> Choice(params char[] parsers) =>
       (TextScanner scanner) =>
       {
          if (parsers.Length == 0)
             return new EmptyToken<char>(scanner.Offset);

          var at = scanner.Offset;

          if (!scanner.StartsWith(parsers))
             return Expected.Char<char>(parsers, at);

          return new Token<char>(at, 1, scanner.Chars.Span[scanner.Offset++]);
       };

   public static Parser<ReadOnlyMemory<char>> Choice(params Regex[] parsers) =>
       (TextScanner scanner) =>
       {
          if (parsers.Length == 0)
             return new EmptyToken<ReadOnlyMemory<char>>(scanner.Offset);

          var at = scanner.Offset;

          if (!scanner.StartsWith(parsers, out var length))
             return Expected.Regex<ReadOnlyMemory<char>>(parsers, at);

          return new Lexeme(at, length, scanner.ReadText(length));
       };

   public static Parser<ReadOnlyMemory<char>> Choice(params string[] parsers) =>
       (TextScanner scanner) =>
       {
          if (parsers.Length == 0)
             return new EmptyToken<ReadOnlyMemory<char>>(scanner.Offset);

          var at = scanner.Offset;

          if (!scanner.StartsWith(parsers, out var length))
             return Expected.Text<ReadOnlyMemory<char>>(parsers, at);

          return new Lexeme(at, length, scanner.ReadText(length));
       };

   public static Parser<T> Choice<T>(params Parser<T>[] parsers) =>
       (TextScanner scanner) =>
       {
          if (parsers.Length == 0)
             return new EmptyToken<T>(scanner.Offset);

          var at = scanner.Offset;
          var errors = new List<(int, string)>();
          IParseResult<T> result;

          foreach (var parser in parsers)
          {
             result = parser(scanner);

             if (result.IsMatch)
                return result;

             scanner.Offset = at;

             errors.Add((result.Offset, result.Why));
          }

          var errorMessage = string.Join(
               " \u2228 ",
               errors.Select(error => $"{{{error.Item2} @ {error.Item1}}}"));

          return Undo<T>(scanner, at, at, errorMessage);
       };

   public static Parser<T> DirectLeftRecursion<T>(Parser<T> parser) =>
       new DLRParser<T>(parser).Apply;

   public static Parser<IList<T>> Exactly<T>(int n, Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          if (n < 0)
             return Expected.NonNegativeInt<IList<T>>(n);

          if (n == 0)
             return new EmptyToken<IList<T>>(scanner.Offset);

          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T> match = new EmptyToken<T>(at);

          for (var i = 0; i < n && (match = parser(scanner)).IsMatch; i++)
             matched.Add(match.Value!);

          if (matched.Count < n)
             return Undo<IList<T>>(scanner, match.Offset, at, match.Why);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<T> FollowedBy<T>(Parser<T> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var followedBy = assertion(scanner);

          if (!followedBy.IsMatch)
             return Undo<T>(scanner, followedBy.Offset, at, followedBy.Why);

          scanner.Offset = at;

          return new EmptyToken<T>(at);
       };

   public static Parser<T> IfFollowedBy<T, A>(Parser<T> parser, Parser<A> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var parsed = parser(scanner);

          if (!parsed.IsMatch)
             return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

          var followedBy = assertion(scanner);

          if (!followedBy.IsMatch)
             return Undo<T>(scanner, followedBy.Offset, at, followedBy.Why);

          scanner.Offset = at + parsed.Length;

          return parsed;
       };

   public static Parser<T> IfNotFollowedBy<T, A>(Parser<T> parser, Parser<A> assertion) =>
      (TextScanner scanner) =>
      {
         var at = scanner.Offset;
         var parsed = parser(scanner);

         if (!parsed.IsMatch)
            return Undo<T>(
               scanner, parsed.Offset, at, parsed.Why);

         var followedBy = assertion(scanner);

         if (followedBy.IsMatch)
            return DidNotExpect.Match<T>(scanner, followedBy.Offset, at);

         scanner.Offset = at + parsed.Length;

         return parsed;
      };

   public static Parser<T> IfNotPrecededBy<T, A>(Parser<T> parser, Parser<A> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var parsed = parser(scanner);

          if (!parsed.IsMatch)
             return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

          var currentOffset = 0;
          IParseResult<A>? precededBy = null;

          scanner.Offset = 0;

          while (currentOffset < at)
          {
             precededBy = assertion(scanner);

             if (precededBy.IsMatch)
             {
                currentOffset += precededBy.Length;
             }
             else
             {
                currentOffset = ++scanner.Offset;
                precededBy = null;
             }
          }

          if (precededBy is null)
          {
             scanner.Offset = at + parsed.Length;
             return parsed;
          }

          if (currentOffset == at)
             return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);

          var shortMatch = scanner.Chars[precededBy.Offset..at];
          var shortParse = assertion(new(shortMatch));

          if (!shortParse.IsMatch)
          {
             scanner.Offset = at + parsed.Length;
             return parsed;
          }

          return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);
       };

   public static Parser<T> IfPrecededBy<T, A>(Parser<T> parser, Parser<A> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var parsed = parser(scanner);

          if (!parsed.IsMatch)
             return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

          var currentOffset = 0;
          IParseResult<A>? precededBy = null;

          scanner.Offset = 0;

          while (currentOffset < at)
          {
             precededBy = assertion(scanner);

             if (precededBy.IsMatch)
                currentOffset += precededBy.Length;
             else
                currentOffset = ++scanner.Offset;
          }

          if (precededBy is null)
             return Expected.Match<T>(scanner, 0, at);

          if (!precededBy.IsMatch)
             return Undo<T>(scanner, precededBy.Offset, at, precededBy.Why);

          if (currentOffset == at)
          {
             scanner.Offset = at + parsed.Length;
             return parsed;
          }

          var shortMatch = scanner.Chars[precededBy.Offset..at];
          var shortParse = assertion(new(shortMatch));

          if (!shortParse.IsMatch)
             return Expected.Match<T>(scanner, precededBy.Offset, at);

          scanner.Offset = at + parsed.Length;
          return parsed;
       };

   public static Parser<T> LeftRecursion<T>(Parser<T> parser) =>
       new ILRParser<T>(parser).Apply;

   public static Parser<T> Island<O, T, C>(Parser<O> open, Parser<T> parser, Parser<C> close) =>
      (TextScanner scanner) =>
      {
         var at = scanner.Offset;
         var opener = open(scanner);

         if (!opener.IsMatch)
            return Undo<T>(scanner, opener.Offset, at, opener.Why);

         var parsed = parser(scanner);

         if (!parsed.IsMatch)
            return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

         var closer = close(scanner);

         if (!closer.IsMatch)
            return Undo<T>(scanner, closer.Offset, at, closer.Why);

         return parsed;
      };

   public static Parser<IList<T>> Join<S, T>(Parser<S> separator, params Parser<T>[] parsers) => (TextScanner scanner) =>
   {
      if (parsers.Length == 0)
         return new EmptyToken<IList<T>>(scanner.Offset);

      var at = scanner.Offset;
      IParseResult<T> result;

      var matched = new List<T>();

      for (var i = 0; i < parsers.Length - 1; i++)
      {
         var parser = parsers[i];
         result = parser(scanner);

         if (!result.IsMatch)
            return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

         matched.Add(result.Value!);

         var sResult = separator(scanner);

         if (!sResult.IsMatch)
            return Undo<IList<T>>(scanner, sResult.Offset, at, sResult.Why);
      }

      result = parsers[^1](scanner);

      if (!result.IsMatch)
         return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

      matched.Add(result.Value!);

      return new Token<IList<T>>(at, scanner.Offset - at, matched);
   };

   public static Parser<IList<T>> Maximum<T>(int n, Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          if (n < 0)
             return Expected.NonNegativeInt<IList<T>>(n);

          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T> match;

          for (var i = 0; i < n && (match = parser(scanner)).IsMatch; i++)
             matched.Add(match.Value!);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<T> Memoize<T>(Parser<T> parser) =>
       new PackratParser<T>(parser).Apply;

   public static Parser<IList<T>> Minimum<T>(int n, Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          if (n < 0)
             return Expected.NonNegativeInt<IList<T>>(n);

          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T>? match = null;

          while ((match = parser(scanner)).IsMatch)
             matched.Add(match.Value!);

          if (matched.Count < n)
             return Undo<IList<T>>(scanner, match.Offset, at, match.Why);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<IList<T>> NotExactly<T>(int n, Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          if (n < 0)
             return Expected.NonNegativeInt<IList<T>>(n);

          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T> match = new EmptyToken<T>(at);

          while (scanner.Offset < scanner.Chars.Length && (match = parser(scanner)).IsMatch)
             matched.Add(match.Value!);

          if (matched.Count == n)
             return DidNotExpect.Match<IList<T>>(scanner, match.Offset, at);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<T> NotFollowedBy<T>(Parser<T> assertion) =>
      (TextScanner scanner) =>
      {
         var at = scanner.Offset;
         var followedBy = assertion(scanner);

         if (followedBy.IsMatch)
            return DidNotExpect.Match<T>(scanner, followedBy.Offset, at);

         scanner.Offset = at;

         return new EmptyToken<T>(at);
      };

   public static Parser<T> NotPrecededBy<T>(Parser<T> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;

          var currentOffset = 0;
          IParseResult<T>? precededBy = null;

          scanner.Offset = 0;

          while (currentOffset < at)
          {
             precededBy = assertion(scanner);

             if (precededBy.IsMatch)
             {
                currentOffset += precededBy.Length;
             }
             else
             {
                currentOffset = ++scanner.Offset;
                precededBy = null;
             }
          }

          if (precededBy is null)
          {
             scanner.Offset = at;

             return new EmptyToken<T>(at);
          }

          if (currentOffset == at)
             return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);

          var shortMatch = scanner.Chars[precededBy.Offset..at];
          var shortParse = assertion(new(shortMatch));

          if (!shortParse.IsMatch)
          {
             scanner.Offset = at;
             return new EmptyToken<T>(at);
          }

          return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);
       };

   public static Parser<T> Optional<T>(Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var match = parser(scanner);

          if (!match.IsMatch)
          {
             scanner.Offset = at;

             return new EmptyToken<T>(at);
          }

          return match;
       };

   public static Parser<T> PrecededBy<T>(Parser<T> assertion) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var currentOffset = 0;
          IParseResult<T>? precededBy = null;

          scanner.Offset = 0;

          while (currentOffset < at)
          {
             precededBy = assertion(scanner);

             if (precededBy.IsMatch)
                currentOffset += precededBy.Length;
             else
                currentOffset = ++scanner.Offset;
          }

          if (precededBy is null)
             return Expected.Match<T>(scanner, 0, at);

          if (!precededBy.IsMatch)
             return Undo<T>(scanner, precededBy.Offset, at, precededBy.Why);

          if (currentOffset > at)
          {
             var shortMatch = scanner.Chars[precededBy.Offset..at];
             var shortParse = assertion(new(shortMatch));

             if (!shortParse.IsMatch)
                return Expected.Match<T>(scanner, precededBy.Offset, at);
          }

          scanner.Offset = at;
          return new EmptyToken<T>(at);
       };

   public static Parser<T> Ref<T>(Func<Parser<T>> parserRef) =>
      new ParserReference<T>(parserRef).Parse;

   public static Parser<IList<T>> Repeat<T>(int min, int max, Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          if (min < 0 || max < min)
             return Expected.ValidRange<IList<T>>(min, max);

          if (min == max && min == 0)
             return new EmptyToken<IList<T>>(scanner.Offset);

          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T>? match = null;

          for (var i = 0; i < max && (match = parser(scanner)).IsMatch; i++)
             matched.Add(match.Value!);

          if (matched.Count < min)
             return Undo<IList<T>>(scanner, match!.Offset, at, match.Why);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<T> Satisfies<T>(Parser<T> parser, Func<T, bool> test) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);

          if (!result.IsMatch)
             return Undo<T>(scanner, result.Offset, at, result.Why);

          if (!test(result.Value!))
             return Expected.ToPass(scanner, result.Value!, at, at);

          return result;
       };

   public static Parser<IList<T>> SeparatedBy<T, S>(Parser<T> parser, Parser<S> separator) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);
          var separation = 0;

          if (!result.IsMatch)
             return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

          var matched = new List<T>();

          while (result.IsMatch)
          {
             matched.Add(result.Value!);

             var currentOffset = scanner.Offset;
             var sResult = separator(scanner);

             if (!sResult.IsMatch)
             {
                scanner.Offset = currentOffset;

                return new Token<IList<T>>(at, currentOffset - at, matched);
             }

             separation = sResult.Length;
             result = parser(scanner);
          }

          scanner.Offset -= separation;

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };

   public static Parser<ReadOnlyMemory<char>> Until<T>(Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);
          var index = at;

          while (!result.IsMatch && scanner.Offset < scanner.Length)
          {
             index++;
             scanner.Offset++;
             result = parser(scanner);
          }

          scanner.Offset = at;

          if (!result.IsMatch)
             return new Token<ReadOnlyMemory<char>>(at, scanner.Length - at, scanner.ReadToEnd());

          return new Token<ReadOnlyMemory<char>>(at, index - at, scanner.ReadText(index - at));
       };

   public static Parser<IList<T>> ZeroOrMore<T>(Parser<T> parser) =>
       (TextScanner scanner) =>
       {
          var matched = new List<T>();
          var at = scanner.Offset;
          IParseResult<T> match;

          while (scanner.Offset < scanner.Chars.Length && (match = parser(scanner)).IsMatch)
             matched.Add(match.Value!);

          return new Token<IList<T>>(at, scanner.Offset - at, matched);
       };
}