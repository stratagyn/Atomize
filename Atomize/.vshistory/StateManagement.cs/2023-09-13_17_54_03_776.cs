using static Atomize.Failure;

namespace Atomize;

public static partial class Parse
{
    public static Parser<U> Bind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Why);

            var boundResult = bind(result.Value!)(scanner);

            if (!boundResult.IsToken)
                return Undo<U>(scanner, boundResult.Offset, at, boundResult.Why);

            return new Lexeme<U>(at, scanner.Offset - at, boundResult.Value!);
        };

    public static Parser<T> Handle<T>(Parser<T> parser, Parser<T> handle) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (result.IsToken)
                return result;

            scanner.Offset = at;

            var handleResult = handle(scanner);

            if (!handleResult.IsToken)
                return Undo<T>(scanner, handleResult.Offset, at, handleResult.Why);

            return new Lexeme<T>(at, scanner.Offset - at, handleResult.Value!);
        };

    public static Parser<U> If<T, U>(Parser<T> parser, Func<T, Parser<U>> bind, Parser<U> handle) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
            {
                scanner.Offset = at;

                var handleResult = handle(scanner);

                if (!handleResult.IsToken)
                    return Undo<U>(scanner, handleResult.Offset, at, handleResult.Why);

                return new Lexeme<U>(at, scanner.Offset - at, handleResult.Value!);
            }

            var boundResult = bind(result.Value!)(scanner);

            if (!boundResult.IsToken)
                return Undo<U>(scanner, boundResult.Offset, at, boundResult.Why);

            return new Lexeme<U>(at, scanner.Offset - at, boundResult.Value!);
        };

    public static Parser<U> Map<T, U>(Parser<T> parser, Func<T, U> map) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Why);

            return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
        };

    public static Parser<(Trial T, Fallback IParseResult<U>)> TryBind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var firstResult = parser(scanner);

            if (!firstResult.IsToken)
                return Undo<(T, IParseResult<U>)>(scanner, firstResult.Offset, at, firstResult.Why);

            var secondResult = bind(firstResult.Value!)(scanner);

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
