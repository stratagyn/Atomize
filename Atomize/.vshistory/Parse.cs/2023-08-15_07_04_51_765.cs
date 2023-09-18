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

    public static Parser<U> As<T, U>(this Parser<T> parser, Func<T, U> map) =>
        (Scanner scanner) =>
        {
            var startingAt = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, startingAt, result.Conflict);

            return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
        };

    public static Parser<U> FMap<T, U>(this Parser<T> parser, Func<T, IParseResult<U>> bind) =>
        (Scanner scanner) =>
        {
            var startingAt = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, startingAt, result.Conflict);

            return bind(result.Value!);
        };

    public static Parser<T> Handle<T>(this Parser<T> parser, Func<IParseResult<T>, IParseResult<T>> handle) =>
        (Scanner scanner) =>
        {
            var startingAt = scanner.Offset;
            var result = parser(scanner);

            if (result.IsToken)
                return result;

            return handle(result);
        };

    public static Parser<U> Then<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (Scanner scanner) =>
        {
            var startingAt = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, startingAt, result.Conflict);

            return bind(result)(scanner);
        };
}