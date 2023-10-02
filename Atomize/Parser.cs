namespace Atomize;

public delegate IParseResult<T> Parser<T>(TextScanner scanner);

internal class ParserReference<T>
{
   private readonly Func<Parser<T>> _ref;
   private Parser<T>? _parser;
   private bool _memoized;

   public ParserReference(Func<Parser<T>> parserRef) => _ref = parserRef;

   public IParseResult<T> Parse(TextScanner scanner)
   {
      if (!_memoized)
      {
         _parser = _ref();
         _memoized = true;
      }

      if (_parser is null)
         return new Failure<T>(scanner.Offset, "Undefined parser");

      return _parser(scanner);
   }
}