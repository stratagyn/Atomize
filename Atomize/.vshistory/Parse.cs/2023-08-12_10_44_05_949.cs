using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(TokenReader reader);


public static partial class Parser
{
    private static readonly ConcurrentDictionary<char, Parser<char>> Characters =
        new();

    private static readonly ConcurrentDictionary<string, Parser<ReadOnlyMemory<char>>> Strings =
        new();

    private static readonly ConcurrentDictionary<string, Parser<ReadOnlyMemory<char>>> Patterns =
        new();

    public static IParseResult<T> EndOfText<T>(TokenReader reader)
    {
        if (reader.Remaining > 0)
            return Expected.EndOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }

    public static Parser<char> Literal(char token) =>
        LiteralCache.Match(token);

    public static Parser<ReadOnlyMemory<char>> Literal(string token) =>
        LiteralCache.Match(token);

    public static Parser<ReadOnlyMemory<char>> Literal(Regex token) =>
        LiteralCache.Match(token);

    public static IParseResult<T> StartOfText<T>(TokenReader reader)
    {
        if (reader.Offset > 0)
            return Expected.StartOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }

    public static Parser<U> Then<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return bind(result)(reader);
        };
}