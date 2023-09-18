using static Atomize.Failure;

namespace Atomize;

public static partial class Parse
{
    public static Parser<T> Choice<T>(params Parser<T>[] parsers) =>
        (TextScanner scanner) =>
        {
            if (parsers.Length == 0)
                return new EmptyToken<T>(scanner.Offset);

            var at = scanner.Offset;
            var errors = new List<(int, string)>();
            IParseResult<T> result;

            foreach (var parser in parsers)
            {
                result = parser(scanner);

                if (result.IsToken)
                    return result;

                scanner.Offset = at;

                errors.Add((result.Offset, result.Why));
            }

            var errorMessage = string.Join(
                " \u2228 ",
                errors.Select(error => $"{{{error.Item2} @ {error.Item1}}}"));

            return Undo<T>(scanner, at, at, errorMessage);
        };

    public static Parser<IList<T>> Exactly<T>(int n, Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            if (n == 0)
                return new EmptyToken<IList<T>>(scanner.Offset);

            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T> match = new EmptyToken<T>(at);

            for (var i = 0; i < n && (match = parser(scanner)).IsToken; i++)
                matched.Add(match.Value!);

            if (matched.Count < n)
                return Undo<IList<T>>(scanner, match.Offset, at, match.Why);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<T> IfFollowedBy<T, F>(Parser<T> parser, Parser<F> assertion) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var parsed = parser(scanner);

            if (!parsed.IsToken)
                return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

            var followedBy = assertion(scanner);

            if (!followedBy.IsToken)
                return Undo<T>(scanner, followedBy.Offset, at, followedBy.Why);

            scanner.Offset = at + parsed.Length;

            return parsed;
        };

    public static Parser<T> IfPrecededBy<T, P>(Parser<T> parser, Parser<P> assertion) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var parsed = parser(scanner);

            if (!parsed.IsToken)
                return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

            var currentOffset = 0;
            IParseResult<P>? precededBy = null;

            scanner.Offset = 0;

            while (currentOffset < at)
            {
                precededBy = assertion(scanner);

                if (precededBy.IsToken)
                    currentOffset += precededBy.Length;
                else
                    currentOffset = ++scanner.Offset;
            }

            if (precededBy is null)
                return Expected.Match<T>(scanner, 0, at);

            if (!precededBy.IsToken)
                return Undo<T>(scanner, precededBy.Offset, at, precededBy.Why);

            if (currentOffset == at)
            {
                scanner.Offset = at + parsed.Length;
                return parsed;
            }

            var shortMatch = scanner.Chars[precededBy.Offset..at];
            var shortParse = parser(new(shortMatch));

            if (!shortParse.IsToken)
                return Expected.Match<T>(scanner, precededBy.Offset, at);

            scanner.Offset = at + parsed.Offset;
            return parsed;
        };

    public static Parser<IList<T>> Join<S, T>(Parser<S> separator, params Parser<T>[] parsers) => (TextScanner scanner) =>
    {
        if (parsers.Length == 0)
            return new EmptyToken<IList<T>>(scanner.Offset);

        var at = scanner.Offset;
        IParseResult<T> result;

        var matched = new List<T>();

        for (var i = 0; i < parsers.Length - 1; i++)
        {
            var parser = parsers[i];
            result = parser(scanner);

            if (!result.IsToken)
                return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

            matched.Add(result.Value!);

            var sResult = separator(scanner);

            if (!sResult.IsToken)
                return Undo<IList<T>>(scanner, sResult.Offset, at, sResult.Why);
        }

        result = parsers[^1](scanner);

        if (!result.IsToken)
            return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

        matched.Add(result.Value!);

        return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
    };

    public static Parser<T> Island<O, T, C>(Parser<O> open, Parser<T> parser, Parser<C> close) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var opener = open(scanner);

            if (!opener.IsToken)
                return Undo<T>(scanner, opener.Offset, at, opener.Why);

            var parsed = parser(scanner);

            if (!parsed.IsToken)
                return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

            var closer = close(scanner);

            if (!closer.IsToken)
                return Undo<T>(scanner, closer.Offset, at, closer.Why);

            return parsed;
        };

    public static Parser<IList<T>> Maximum<T>(int n, Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T> match;

            for (var i = 0; i < n && (match = parser(scanner)).IsToken; i++)
                matched.Add(match.Value!);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<IList<T>> Minimum<T>(int n, Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T>? match = null;

            while ((match = parser(scanner)).IsToken)
                matched.Add(match.Value!);

            if (matched.Count < n)
                return Undo<IList<T>>(scanner, match.Offset, at, match.Why);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<IList<T>> NotExactly<T>(int n, Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            if (n < 0)
                return Expected.NonNegativeInt<IList<T>>(n);

            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T> match = new EmptyToken<T>(at);

            while (scanner.Offset < scanner.Chars.Length && (match = parser(scanner)).IsToken)
                matched.Add(match.Value!);

            if (matched.Count == n)
                return DidNotExpect.Match<IList<T>>(scanner, match.Offset, at);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<T> NotFollowedBy<T, F>(Parser<T> parser, Parser<F> assertion) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var parsed = parser(scanner);

            if (!parsed.IsToken)
                return Undo<T>(
                    scanner, parsed.Offset, at, parsed.Why);

            var followedBy = assertion(scanner);

            if (followedBy.IsToken)
                return DidNotExpect.Match<T>(scanner, followedBy.Offset, at);

            scanner.Offset = at + parsed.Length;

            return parsed;
        };

    public static Parser<T> NotPrecededBy<T, P>(Parser<T> parser, Parser<P> assertion) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var parsed = parser(scanner);

            if (!parsed.IsToken)
                return Undo<T>(scanner, parsed.Offset, at, parsed.Why);

            var currentOffset = 0;
            IParseResult<P>? precededBy = null;

            scanner.Offset = 0;

            while (currentOffset < at)
            {
                precededBy = assertion(scanner);

                if (precededBy.IsToken)
                {
                    currentOffset += precededBy.Length;
                }
                else
                {
                    currentOffset = ++scanner.Offset;
                    precededBy = null;
                }
            }

            if (precededBy is null)
            {
                scanner.Offset = at + parsed.Length;
                return parsed;
            }

            if (currentOffset == at)
                return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);

            var shortMatch = scanner.Chars[precededBy.Offset..at];
            var shortParse = parser(new(shortMatch));

            if (!shortParse.IsToken)
            {
                scanner.Offset = at + parsed.Length;
                return parsed;
            }

            return DidNotExpect.Match<T>(scanner, precededBy.Offset, at);
        };

    public static Parser<T> Optional<T>(Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var match = parser(scanner);

            if (!match.IsToken)
            {
                scanner.Offset = at;

                return new EmptyToken<T>(at);
            }

            return match;
        };

    public static Parser<(IParseResult<T>, IParseResult<U>)> Or<T, U>(Parser<T> parser1, Parser<U> parser2) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var first = parser1(scanner);

            if (first.IsToken)
            {
                var result = parser2(scanner);
                var length = result.IsToken ? scanner.Offset - at : first.Length;

                if (!result.IsToken)
                    scanner.Offset = at + first.Length;

                return new Lexeme<(IParseResult<T>, IParseResult<U>)>(
                    at,
                    length,
                    (first, result));
            }

            scanner.Offset = at;

            var second = parser2(scanner);

            if (!second.IsToken)
                return Undo<(IParseResult<T>, IParseResult<U>)>(
                    scanner,
                    second.Offset,
                    at,
                    $"{{@ {first.Offset} : {first.Why}}} \u2228 {{@ {second.Offset} : {second.Why}}}");

            return new Lexeme<(IParseResult<T>, IParseResult<U>)>(
                at,
                second.Length, (first, second));
        };

    public static Parser<IList<T>> Range<T>(int min, int max, Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            if (min < 0 || max < min)
                return Expected.ValidRange<IList<T>>(min, max);

            if (min == max && min == 0)
                return new EmptyToken<IList<T>>(scanner.Offset);

            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T>? match = null;

            for (var i = 0; i < max && (match = parser(scanner)).IsToken; i++)
                matched.Add(match.Value!);

            if (matched.Count < min)
                return Undo<IList<T>>(scanner, match!.Offset, at, match.Why);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<T> Satisfies<T>(Parser<T> parser, Predicate<T> test) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<T>(scanner, result.Offset, at, result.Why);

            if (!test(result.Value!))
                return Expected.ToPass(scanner, result.Value!, at, at);

            return result;
        };

    public static Parser<IList<T>> SeparatedBy<T, S>(Parser<T> parser, Parser<S> separator) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);
            var separation = 0;

            if (!result.IsToken)
                return Undo<IList<T>>(scanner, result.Offset, at, result.Why);

            var matched = new List<T>();

            while (result.IsToken)
            {
                matched.Add(result.Value!);

                var currentOffset = scanner.Offset;
                var sResult = separator(scanner);

                if (!sResult.IsToken)
                {
                    scanner.Offset = currentOffset;

                    return new Lexeme<IList<T>>(at, currentOffset - at, matched);
                }

                separation = sResult.Length;
                result = parser(scanner);
            }

            scanner.Offset -= separation;

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };

    public static Parser<ReadOnlyMemory<char>> Until<T>(Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);
            var index = at;

            while (!result.IsToken && scanner.Offset < scanner.Length)
            {
                index++;
                scanner.Offset++;
                result = parser(scanner);
            }

            scanner.Offset = at;

            if (!result.IsToken)
                return new Lexeme<ReadOnlyMemory<char>>(at, scanner.Length - at, scanner.ReadToEnd());

            return new Lexeme<ReadOnlyMemory<char>>(at, index - at, scanner.ReadText(index - at));
        };

    public static Parser<IList<T>> ZeroOrMore<T>(Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            var matched = new List<T>();
            var at = scanner.Offset;
            IParseResult<T> match;

            while (scanner.Offset < scanner.Chars.Length && (match = parser(scanner)).IsToken)
                matched.Add(match.Value!);

            return new Lexeme<IList<T>>(at, scanner.Offset - at, matched);
        };
}