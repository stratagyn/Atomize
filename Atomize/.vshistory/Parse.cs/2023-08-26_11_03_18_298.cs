using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(TextScanner scanner);

public static partial class Parse
{
    public static Parser<U> Bind<T, U>(Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Conflict);

            var boundResult = bind(result)(scanner);

            if (!boundResult.IsToken)
                return Undo<U>(scanner, boundResult.Offset, at, boundResult.Conflict);

            return new Lexeme<U>(at, scanner.Offset - at, boundResult.Value!);
        };

    public static Parser<T> Handle<T>(Parser<T> parser, Func<IParseResult<T>, Parser<T>> handle) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (result.IsToken)
                return result;

            scanner.Offset = at;

            return handle(result)(scanner);
        };

    public static Parser<T> Lazy<T>(Func<Parser<T>> lazyParser)
    {
        var parser = lazyParser();

        IParseResult<T> Parse(TextScanner scanner) => parser(scanner);

        return Parse;
    }

    public static Parser<U> Map<T, U>(Parser<T> parser, Func<T, U> map) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Conflict);

            return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
        };

    public static Parser<(T, IParseResult<U>)> TryBind<T, U>(Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var firstResult = parser(scanner);

            if (!firstResult.IsToken)
                return Undo<(T, IParseResult<U>)>(scanner, firstResult.Offset, at, firstResult.Conflict);

            var secondResult = bind(firstResult)(scanner);

            if (!secondResult.IsToken)
            {
                scanner.Offset = at + firstResult.Length;

                return new Lexeme<(T, IParseResult<U>)>(
                    at,
                    firstResult.Length,
                    (firstResult.Value!, secondResult));
            }

            return new Lexeme<(T, IParseResult<U>)>(
                at,
                firstResult.Length + secondResult.Length,
                (firstResult.Value!, secondResult));
        };
}