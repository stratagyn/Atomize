using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(Scanner scanner);

public static partial class Parser
{
    public static IParseResult<T> EndOfText<T>(Scanner scanner)
    {
        if (scanner.Remaining > 0)
            return Expected.EndOfText<T>(scanner.Offset);

        return new EmptyToken<T>(scanner.Offset);
    }

    public static IParseResult<T> StartOfText<T>(Scanner scanner)
    {
        if (scanner.Offset > 0)
            return Expected.StartOfText<T>(scanner.Offset);

        return new EmptyToken<T>(scanner.Offset);
    }

    public static Parser<U> Bind<T, U>(Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (Scanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Conflict);

            return bind(result)(scanner);
        };

    public static Parser<U> FMap<T, U>(Parser<T> parser, Func<T, IParseResult<U>> bind) =>
        (Scanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Conflict);

            return bind(result.Value!);
        };

    public static Parser<T> Handle<T>(Parser<T> parser, Func<IParseResult<T>, IParseResult<T>> handle) =>
        (Scanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (result.IsToken)
                return result;

            return handle(result);
        };

    public static Parser<U> Map<T, U>(Parser<T> parser, Func<T, U> map) =>
        (Scanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, at, result.Conflict);

            return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
        };

    public static Parser<(T, IParseResult<U>)> TryBind<T, U>(Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (Scanner scanner) =>
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

            return new Lexeme<(T, U?)>(
                at, 
                firstResult.Length + secondResult.Length, 
                (firstResult.Value!, secondResult.Value));
        };
}