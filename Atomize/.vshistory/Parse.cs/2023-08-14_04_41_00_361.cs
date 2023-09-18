using System.Reflection.PortableExecutable;
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

    public static IParseResult<T> StartOfText<T>(TokenReader reader)
    {
        if (reader.Offset > 0)
            return Expected.StartOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }

    public static Parser<U> As<T, U>(this Parser<T> parser, Func<T, U> map) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return new Token<U>(result.Offset, result.Length, map(result.SemanticValue!));
        };

    public static Parser<U> FMap<T, U>(this Parser<T> parser, Func<T, IParseResult<U>> bind) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return bind(result.SemanticValue!);
        };

    public static Parser<T> Handle<T>(this Parser<T> parser, Func<IParseResult<T>, IParseResult<T>> handle) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (result.IsToken)
                return result;

            return handle(result);
        };

    public static Parser<U> Then<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return bind(result)(reader);
        };

    public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> map) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return new Token<U>(result.Offset, result.Length, map(result.SemanticValue!));
        };

    public static Parser<U> SelectMany<T, U, R>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind, Func<T, U, R> map) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return bind(result)(reader);
        };

    public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> test) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<T>(reader, result.Offset, at, result.Conflict);

            if (!test(result.SemanticValue!))
                return Expected.ToPass(reader, result.SemanticValue!, at, at);

            return result;
        };
}