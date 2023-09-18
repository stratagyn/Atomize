using System.Text.RegularExpressions;
using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(TokenReader reader);


public static partial class Parser
{
    public static IParseResult<T> EndOfText<T>(TokenReader reader)
    {
        if (reader.Remaining > 0)
            return Expected.EndOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }

    public static IParseResult<char> Literal(TokenReader reader)
    {
        if (reader.Remaining == 0)
            return DidNotExpect.EndOfText<char>(reader.Offset);

        var token = reader.Read();

        return new Character(reader.Offset, token);
    }

    public static Parser<ReadOnlyMemory<char>> Literal(int n) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<ReadOnlyMemory<char>>(n);

            if (n > reader.Remaining)
                return Expected.EnoughCharacters<ReadOnlyMemory<char>>(reader.Offset);

            var token = reader.Read(n);

            return new Text(reader.Offset, token);
        };

    public static Parser<char> Literal(char token) =>
        (TokenReader reader) =>
        {
            if (reader.Remaining == 0)
                return DidNotExpect.EndOfText<char>(reader.Offset);

            return reader.StartsWith(token)
                ? new Character(reader.Offset, reader.ReadChar())
                : Expected.Char<char>(token, reader.Offset);
        };

    public static Parser<ReadOnlyMemory<char>> Literal(string token) =>
        (TokenReader reader) =>
        {
            if (reader.Remaining < token.Length)
                return DidNotExpect.EndOfText<ReadOnlyMemory<char>>(reader.Text.Length);

            return reader.StartsWith(token)
                ? new Text(reader.Offset, reader.ReadText(token.Length))
                : Expected.Text<ReadOnlyMemory<char>>(token, reader.Offset);
        };

    public static Parser<ReadOnlyMemory<char>> Literal(Regex token) =>
        (TokenReader reader) =>
            reader.StartsWith(token, out var length)
                ? new Text(reader.Offset, reader.ReadText(length))
                : Expected.Regex<ReadOnlyMemory<char>>(token, reader.Offset);

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