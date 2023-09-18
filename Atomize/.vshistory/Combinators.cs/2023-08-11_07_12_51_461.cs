using System;
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

            return ParseResult.Match(at, at - reader.Offset, matched);
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
            var startingOffset = reader.Offset;
            IList<T> matched = new List<T>();
            ParseResult<T> result;

            foreach (var parser in parsers)
            {
                result = parser(reader);

                if (!result.IsToken)
                    return Manage.Undo<IList<T>>(
                        reader, result.Offset, startingOffset, result.ReasonForFailure);

                matched.Add(result.Token!);
            }

            return ParseResult.Succeed(startingOffset, reader.Text[startingOffset..reader.Offset], matched);
        };
}
