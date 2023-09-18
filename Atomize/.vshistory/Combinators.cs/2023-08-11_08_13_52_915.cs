﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Failure;
using static Atomize.Utilities;

namespace Atomize;

public static partial class Parser
{
    public static Parser<T> Choose<T>(Parser<T> rule, params Parser<T>[] rules) => 
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var errors = new List<(int, string)>();
            var result = rule(reader);

            if (result.IsToken)
                return result;

            errors.Add((result.Offset, result.Conflict));

            foreach (var rule in rules)
            {
                result = rule(reader);

                if (result.IsToken)
                    return result;

                reader.Offset = at;

                errors.Add((result.Offset, result.Conflict));
            }

            var errorMessage = string.Join(
                " \u2228 ",
                errors.Select(error => $"{{{error.Item2} @ {error.Item1}}}"));

            return Undo<T>(reader, at, at, errorMessage);
        };

    public static Parser<IList<T>> Exactly<T>(int n, Parser<T> token) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            if (n == 0)
                return new EmptyToken<IList<T>>(reader.Offset);

            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T> match = new EmptyToken<T>(at);

            for (var i = 0; i < n && (match = token(reader)).IsToken; i++)
                matched.Add(match.SemanticValue!);

            if (matched.Count < n)
                return Undo<IList<T>>(reader, match.Offset, at, match.Conflict);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser<IList<T>> Fallback<T>(params Parser<T>[] parsers) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;

            if (parsers.Length == 0)
                return new EmptyToken<IList<T>>(at);

            var first = parsers[0](reader);

            if (!first.IsToken)
                return Undo<IList<T>>(reader, first.Offset, at, first.Conflict);

            IList<T> matched = new List<T>() { first.SemanticValue! };

            for (var i = 1; i < parsers.Length; i++)
            {
                var result = parsers[i](reader);

                if (!result.IsToken)
                {
                    reader.Offset = at + first.Length;

                    return ParseResult.Match<IList<T>>(
                        at,
                        first.Length,
                        UnitArray(first.SemanticValue!));
                }

                if (result.Length > 0)
                    matched.Add(result.SemanticValue!);
            }

            return ParseResult.Match(at, reader.Offset - at, matched);
        };

    public static Parser<T> IfFollowedBy<T>(Parser<T> parser, Parser<T> token) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var parsed = parser(reader);

            if (!parsed.IsToken)
                return Undo<T>(reader, parsed.Offset, at, parsed.Conflict);

            var parsedOffset = reader.Offset;
            var follow = token(reader);

            if (!follow.IsToken)
                return Undo<T>(reader, follow.Offset, at, follow.Conflict);

            reader.Offset = parsedOffset;

            return parsed;
        };

    public static Parser<T> IfPrecededBy<T>(Parser<T> parser, Parser<T> token) => 
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var parsed = parser(reader);

            if (!parsed.IsToken)
                return Undo<T>(reader, parsed.Offset, at, parsed.Conflict);

            var parsedOffset = reader.Offset;
            var currentOffset = 0;
            var matched = false;

            reader.Offset = 0;

            IParseResult<T>? result = null;

            while (currentOffset < at)
            {
                var result = token(reader);
                matched = result.IsToken;

                if (result.IsToken)
                    currentOffset += result.Length;
                else
                    reader.Offset++;
            }

            if (!matched)
            {
                reader.Offset = parsedOffset;
                return parsed;
            }

            var matchStart = 

            return ParseResult.Succeed(startingOffset, result.Match, "");
        };

    public static IParseResult<T> Ignore<T>(TokenReader reader) =>
        reader.Remaining == 0
        ? DidNotExpect.EndOfText<T>(reader.Offset)
        : new EmptyToken<T>(reader.Offset++);

    public static Parser<T> Ignore<T>(int n) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<T>(n);

            if (reader.Remaining < n)
                return DidNotExpect.EndOfText<T>(reader.Text.Length);

            reader.Offset += n;

            return new EmptyToken<T>(reader.Offset - n);
        };

    public static Parser<T> Ignore<T>(Parser<T> parser) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<T>(reader, result.Offset, at, result.Conflict);

            return new EmptyToken<T>(at);
        };

    public static Parser<T> Island<O, T, C>(Parser<O> open, Parser<T> token, Parser<C> close) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var opener = open(reader);

            if (!opener.IsToken)
                return Undo<T>(reader, opener.Offset, at, opener.Conflict);

            var parsed = token(reader);

            if (!parsed.IsToken)
                return Undo<T>(reader, parsed.Offset, at, parsed.Conflict);

            var closer = close(reader);

            if (!closer.IsToken)
                return Undo<T>(reader, closer.Offset, at, closer.Conflict);

            return parsed;
        };

    public static Parser<T> NotFollowedBy<T>(Parser<T> parser, Parser<T> token) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var parsed = parser(reader);

            if (!parsed.IsToken)
                return Undo<T>(
                    reader, parsed.Offset, at, parsed.Conflict);

            var parsedOffset = reader.Offset;
            var follow = token(reader);

            if (follow.IsToken)
                return DidNotExpect.Match<T>(reader, follow.Offset, at);

            reader.Offset = parsedOffset;

            return parsed;
        };

    public static Parser<IList<T>> Sequence<T>(params Parser<T>[] parsers) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            IList<T> matched = new List<T>();
            IParseResult<T> result;

            foreach (var parser in parsers)
            {
                result = parser(reader);

                if (!result.IsToken)
                    return Undo<IList<T>>(reader, result.Offset, at, result.Conflict);

                matched.Add(result.SemanticValue!);
            }

            return ParseResult.Match(at, reader.Offset - at, matched);
        };
}
