using static Atomize.Failure;

namespace Atomize
{

    public delegate IParseResult<T> Parser<T>(TextScanner scanner);

    public static partial class Parse
    {
        public static Parser<U> Bind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (!result.IsToken)
                    return Undo<U>(scanner, result.Offset, at, result.Conflict);

                var boundResult = bind(result.Value!)(scanner);

                if (!boundResult.IsToken)
                    return Undo<U>(scanner, boundResult.Offset, at, boundResult.Conflict);

                return new Lexeme<U>(at, scanner.Offset - at, boundResult.Value!);
            };

        public static Parser<T> Handle<T>(Parser<T> parser, Func<T, Parser<T>> handle) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (result.IsToken)
                    return result;

                scanner.Offset = at;

                var handleResult = handle(result.Value!)(scanner);

                if (!handleResult.IsToken)
                    return Undo<T>(scanner, handleResult.Offset, at, handleResult.Conflict);

                return new Lexeme<T>(at, scanner.Offset - at, handleResult.Value!);
            };

        public static Parser<U> Map<T, U>(Parser<T> parser, Func<T, U> map) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (!result.IsToken)
                    return Undo<U>(scanner, result.Offset, at, result.Conflict);

                return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
            };

        public static Parser<(T, IParseResult<U>)> TryBind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
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
}

namespace Atomize.Ext
{
    public static class Parser
    {
        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> map) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (!result.IsToken)
                    return Undo<U>(scanner, result.Offset, at, result.Conflict);

                return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
            };

        public static Parser<U> SelectMany<T, U>(this Parser<T> parser, Func<T, Parser<U>> bind) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (!result.IsToken)
                    return Undo<U>(scanner, result.Offset, at, result.Conflict);

                var boundResult = bind(result.Value!)(scanner);

                if (!boundResult.IsToken)
                    return Undo<U>(scanner, boundResult.Offset, at, boundResult.Conflict);

                return new Lexeme<U>(result.Offset, scanner.Offset - at, boundResult.Value!);
            };

        public static Parser<R> SelectMany<T, U, R>(this Parser<T> parser, Func<T, Parser<U>> bind, Func<T, U, R> map) =>
            (TextScanner scanner) =>
            {
                var at = scanner.Offset;
                var result = parser(scanner);

                if (!result.IsToken)
                    return Undo<R>(scanner, result.Offset, at, result.Conflict);

                var boundResult = bind(result.Value!)(scanner);

                if (!boundResult.IsToken)
                    return Undo<R>(scanner, boundResult.Offset, at, boundResult.Conflict);

                return new Lexeme<R>(result.Offset, scanner.Offset - at, map(result.Value!, boundResult.Value!));
            };

        public static 
    }
}