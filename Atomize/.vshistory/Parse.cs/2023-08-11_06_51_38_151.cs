﻿using static Atomize.Failure;

namespace Atomize;

public delegate IParseResult<T> Parser<T>(TokenReader reader);


public static class Parser
{
    public static Parser<U> After<T, U>(this Parser<T> parser, Func<IParseResult<T>, Parser<U>> bind) =>
        (TokenReader reader) =>
        {
            var startingOffset = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, startingOffset, result.ReasonForFailure);

            return bind(result)(reader);
        };

    public static IParseResult<T> EOT<T>(TokenReader reader)
    {
        if (reader.Remaining > 0)
            return Expected.EndOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }

    public static IParseResult<T> SOT<T>(TokenReader reader)
    {
        if (reader.Offset > 0)
            return Expected.StartOfText<T>(reader.Offset);

        return new EmptyToken<T>(reader.Offset);
    }
}