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
            IParseResult<T>? result = null;

            reader.Offset = 0;

            while (currentOffset < at)
            {
                result = token(reader);

                if (result.IsToken)
                    currentOffset += result.Length;
                else
                    currentOffset = ++reader.Offset;
            }

            if (result is null)
                return Expected.Match<T>(reader, 0, at);

            if (!result.IsToken)
                return Undo<T>(reader, result.Offset, at, result.Conflict);

            if (currentOffset == at)
            {
                reader.Offset = parsedOffset;
                return parsed;
            }

            var shortMatch = reader.Text[result.Offset..at];
            var shortParse = token(new(shortMatch));

            if (!shortParse.IsToken)
                return Expected.Match<T>(reader, result.Offset, at);

            reader.Offset = parsedOffset;
            return parsed;
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

    public static Parser<IList<T>> Interleave<T>(Parser<T> separator, params Parser<T>[] tokens) => (TokenReader reader) =>
    {
        if (tokens.Length == 0)
            return new EmptyToken<IList<T>>(reader.Offset);

        var at = reader.Offset;
        IList<T> matched = new List<T>();
        IParseResult<T> result;

        for (var i = 0; i < tokens.Length - 1; i++)
        {
            var token = tokens[i];
            result = token(reader);

            if (!result.IsToken)
                return Undo<IList<T>>(reader, result.Offset, at, result.Conflict);

            matched.Add(result.SemanticValue!);

            var sResult = separator(reader);

            if (!sResult.IsToken)
                return Undo<IList<T>>(reader, sResult.Offset, at, sResult.Conflict);
        }

        result = tokens[^1](reader);

        if (!result.IsToken)
            return Undo<IList<T>>(reader, result.Offset, at, result.Conflict);

        matched.Add(result.SemanticValue!);

        return new Token<IList<T>>(at, reader.Offset - at, matched);
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

    public static Parser<IList<T>> LongestPrefix<T>(params Parser<T>[] parsers) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;

            if (parsers.Length == 0)
                return new EmptyToken<IList<T>>(at);

            var result = parsers[0](reader);

            if (!result.IsToken)
                return Undo<IList<T>>(reader, result.Offset, at, result.Conflict);

            IList<T> matched = new List<T>() { result.SemanticValue! };

            for (var i = 1; i < parsers.Length; i++)
            {
                result = parsers[i](reader);

                if (!result.IsToken)
                    return new Token<IList<T>>(at, reader.Offset - at, matched);

                if (result.Length > 0)
                    matched.Add(result.SemanticValue!);
            }

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser<U> Map<T, U>(Func<T, U> f, Parser<T> parser) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var result = parser(reader);

            if (!result.IsToken)
                return Undo<U>(reader, result.Offset, at, result.Conflict);

            return new Token<U>(result.Offset, result.Length, f(result.SemanticValue!));
        };

    public static Parser<IList<T>> Maximum<T>(int n, Parser<T> token) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T> match;

            for (var i = 0; i < n && (match = token(reader)).IsToken; i++)
                matched.Add(match.SemanticValue!);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser<IList<T>> Minimum<T>(int n, Parser<T> token) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T>? match = null;

            for (var i = 0; i < n && (match = token(reader)).IsToken; i++)
                matched.Add(match.SemanticValue!);

            if (match is null)
                return new EmptyToken<IList<T>>(at);

            if (matched.Count < n)
                return Undo<IList<T>>(reader, match.Offset, at, match.Conflict);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser<IList<T>> NotExactly<T>(int n, Parser<T> token) =>
        (TokenReader reader) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T> match = new EmptyToken<T>(at);

            while(reader.Remaining > 0 && (match = token(reader)).IsToken)
                matched.Add(match.SemanticValue!);

            if (matched.Count == n)
                return DidNotExpect.Match<IList<T>>(reader, match.Offset, at);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
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

    public static Parser<T> NotPrecededBy<T>(Parser<T> parser, Parser<T> token) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var parsed = parser(reader);

            if (!parsed.IsToken)
                return Undo<T>(reader, parsed.Offset, at, parsed.Conflict);

            var parsedOffset = reader.Offset;
            var currentOffset = 0;
            IParseResult<T>? result = null;

            reader.Offset = 0;

            while (currentOffset < at)
            {
                result = token(reader);

                if (result.IsToken)
                {
                    currentOffset += result.Length;
                }
                else
                {
                    currentOffset = ++reader.Offset;
                    result = null;
                }
            }

            if (result is null)
            {
                reader.Offset = parsedOffset;
                return parsed;
            }

            if (currentOffset == at)
                return DidNotExpect.Match<T>(reader, result.Offset, at);

            var shortMatch = reader.Text[result.Offset..at];
            var shortParse = token(new(shortMatch));

            if (!shortParse.IsToken)
            {
                reader.Offset = parsedOffset;
                return parsed;
            }

            return DidNotExpect.Match<T>(reader, result.Offset, at);
        };

    public static Parser<T> Optional<T>(Parser<T> token) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var match = token(reader);

            if (!match.IsToken)
            {
                reader.Offset = at;

                return new EmptyToken<T>(at);
            }

            return match;
        };

    public static Parser<(T, T?)> Or<T>(Parser<T> rule1, Parser<T> rule2) => 
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var first = rule1(reader);

            if (first.IsToken)
            {
                var result = rule2(reader);

                if (!result.IsToken)
                {
                    reader.Offset = at + first.Length;

                    return new Token<(T, T?)>(at, first.Length, (first.SemanticValue!, default));
                }

                return new Token<(T, T?)>(at, reader.Offset - at, (first.SemanticValue!, result.SemanticValue!));
            }

            reader.Offset = at;

            var result2 = rule2(reader);

            if (!result2.IsToken)
                return Undo<(T, T?)>(reader, result2.Offset, )
                return new Failure(
                    $"{first.ReasonForFailure} => {result2.ReasonForFailure}",
                    at);

            return result2;
        };

    public static Parser<IList<T>> Range<T>(int min, int max, Parser<T> token) =>
        (TokenReader reader) =>
        {
            if (min < 0 || max < min)
                return Expected.ValidRange<IList<T>>(min, max);

            if (min == max && min == 0)
                return new EmptyToken<IList<T>>(reader.Offset);

            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T>? match = null;

            for (var i = 0; i < max && (match = token(reader)).IsToken; i++)
                matched.Add(match.SemanticValue!);

            if (matched.Count < min)
                return Undo<IList<T>>(reader, match!.Offset, at, match.Conflict);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser<T> Satisfies<T>(Parser<T> rule, Predicate<T> cond) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var result = rule(reader);

            if (!result.IsToken)
                return Undo<T>(reader, result.Offset, at, result.Conflict);

            if (!cond(result.SemanticValue!))
                return Expected.ToPass(result.SemanticValue!, at);

            return result;
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

    public static Parser<IList<T>> Split<T>(Parser<T> separator, Parser<T> token) =>
        (TokenReader reader) =>
        {
            var at = reader.Offset;
            var result = token(reader);
            var separation = 0;

            if (!result.IsToken)
                return Undo<IList<T>>(reader, result.Offset, at, result.Conflict);

            IList<T> matched = new List<T>();

            while (result.IsToken)
            {
                matched.Add(result.SemanticValue!);

                var currentOffset = reader.Offset;
                var sResult = separator(reader);

                if (!sResult.IsToken)
                {
                    reader.Offset = currentOffset;

                    return new Token<IList<T>>(at, currentOffset - at, matched);
                }

                separation = sResult.Length;
                result = token(reader);
            }

            reader.Offset -= separation;

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };

    public static Parser Until(Parser token) => (in TokenReader reader) =>
    {
        var offset = reader.Offset;
        var result = token(reader);
        var index = offset;

        while (!result.IsSuccess && reader.Remaining > 0)
        {
            index++;
            reader.Advance();
            result = token(reader);
        }

        reader.Backtrack(reader.Offset - offset);

        if (!result.IsSuccess)
            return new Lexeme(reader.ReadToEnd(), offset);

        return new Lexeme(reader.Read(index - offset), offset);
    };

    public static Parser<IList<T>> ZeroOrMore<T>(Parser<T> token) =>
        (TokenReader reader) =>
        {
            IList<T> matched = new List<T>();
            var at = reader.Offset;
            IParseResult<T> match;

            while (reader.Remaining > 0 && (match = token(reader)).IsToken)
                matched.Add(match.SemanticValue!);

            return new Token<IList<T>>(at, reader.Offset - at, matched);
        };
}
