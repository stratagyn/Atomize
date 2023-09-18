using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(ref Scanner scanner);

public static partial class Parser
{
    public static IParseResult<T> EndOfText<T>(ref Scanner scanner)
    {
        if (scanner.Remaining > 0)
            return Expected.EndOfText<T>(scanner.Offset);

        return new EmptyToken<T>(scanner.Offset);
    }

    public static IParseResult<T> StartOfText<T>(ref Scanner scanner)
    {
        if (scanner.Offset > 0)
            return Expected.StartOfText<T>(scanner.Offset);

        return new EmptyToken<T>(scanner.Offset);
    }

    public static Parser<U> As<T, U>(this Parser<T> parser, Func<T, U> map) =>
        (ref Scanner scanner) =>
        {
            var startingOffset = scanner.Offset;
            var result = parser(ref scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, startingOffset, result.Conflict);

            return new Lexeme<U>(result.Offset, result.Length, map(result.Value!));
        };

    public static Parser<U> FMap<T, U>(this Parser<T> parser, Func<T, IParseResult<U>> bind) =>
        (ref Scanner scanner) =>
        {
            var startingOffset = scanner.Offset;
            var result = parser(ref scanner);

            if (!result.IsToken)
                return Undo<U>(scanner, result.Offset, startingOffset, result.Conflict);

            return bind(result.Value!);
        };

    public static Parser<T> Handle<T>(this Parser<T> parser, Func<IParseResult<T>, IParseResult<T>> handle) =>
        (ref Scanner scanner) =>
        {
            var startingOffset = scanner.Offset;
            var result = parser(ref scanner);

            if (result.IsToken)
                return result;

            return handle(result);
        };

    public static Parser<U> Then<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (ref Scanner scanner) =>
        {
            var startingOffset = scanner.Offset;
            var result = parser(ref scanner);

            if (!result.IsToken)
                return Undo<U>(ref scanner, result.Offset, startingOffset, result.Conflict);

            return bind(result)(ref scanner);
        };
}