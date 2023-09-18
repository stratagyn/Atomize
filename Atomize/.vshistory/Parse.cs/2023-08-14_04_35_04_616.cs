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

    public static Parser<U> SelectMany<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.Conflict);

            return bind(result)(reader);
        };

    public static Parser<R> SelectMany<T, U, R>(Parser<T> first, Parser<U> second, Func<(T, U)> map) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var firstResult = first(reader);

            if (!firstResult.IsToken)
                return Undo<R>(reader, firstResult.Offset, at, firstResult.Conflict);

            var secondResult = second(reader);

            if (!secondResult.IsToken)
                return Undo<R>(reader, secondResult.Offset, at, secondResult.Conflict);

            return new Token<R>(
                at, reader.Offset - at, map(firstResult.SemanticValue!, secondResult.SemanticValue!));
        };

    public static Parser<(T, U, V)> Sequence<T, U, V>(Parser<T> first, Parser<U> second, Parser<V> third) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var firstResult = first(reader);

            if (!firstResult.IsToken)
                return Undo<(T, U, V)>(reader, firstResult.Offset, at, firstResult.Conflict);

            var secondResult = second(reader);

            if (!secondResult.IsToken)
                return Undo<(T, U, V)>(reader, secondResult.Offset, at, secondResult.Conflict);

            var thirdResult = third(reader);

            if (!thirdResult.IsToken)
                return Undo<(T, U, V)>(reader, thirdResult.Offset, at, thirdResult.Conflict);

            return new Token<(T, U, V)>(
                at,
                reader.Offset - at,
                (firstResult.SemanticValue!, secondResult.SemanticValue!, thirdResult.SemanticValue!));
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