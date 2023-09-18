using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static Atomize.Failure;

using Characters = System.ReadOnlyMemory<char>;
using CharParsers = System.Collections.Concurrent.ConcurrentDictionary<char, Atomize.Parser<char>>;
using PatternParsers = System.Collections.Concurrent.ConcurrentDictionary<char, Atomize.Parser<char>>;

namespace Atomize;

public static partial class Parse
{
    private static readonly ConcurrentDictionary<char, Parser<char>> Chars =
        new();

    private static readonly ConcurrentDictionary<char, Parser<char>> IChars =
        new();

    private static readonly ConcurrentDictionary<string, Parser<Characters>> Patterns =
        new();

    private static readonly ConcurrentDictionary<string, Parser<string>> IPatterns =
        new();

    private static readonly ConcurrentDictionary<string, Parser<Characters>> Strings =
        new();

    private static readonly ConcurrentDictionary<string, Parser<string>> IStrings =
        new();
    

    public static IParseResult<T> Empty<T>(TextScanner scanner) =>
        new EmptyToken<T>(scanner.Offset);

    public static IParseResult<char> EndOfText(TextScanner scanner)
    {
        if (scanner.Offset < scanner.Chars.Length)
            return Expected.EndOfText<char>(scanner.Offset);

        return new EmptyToken<char>(scanner.Offset);
    }

    public static Parser<char> Ignore(char parser) =>
        IChars.GetOrAdd(parser,
            (TextScanner scanner) =>
            {
                if (scanner.Offset == scanner.Chars.Length)
                    return DidNotExpect.EndOfText<char>(scanner.Offset);

                return scanner.StartsWith(parser)
                    ? new EmptyToken<char>(scanner.Offset++)
                    : Expected.Char<char>(parser, scanner.Offset);
            });

    public static Parser<string> Ignore(string parser) =>
        IStrings.GetOrAdd(
            parser,
            (TextScanner scanner) =>
            {
                if ((scanner.Chars.Length - scanner.Offset) < parser.Length)
                    return DidNotExpect.EndOfText<string>(scanner.Offset);

                if (!scanner.StartsWith(parser))
                    return Expected.Text<string>(parser, scanner.Offset);

                scanner.Offset += parser.Length;

                return new EmptyToken<string>(scanner.Offset - parser.Length);
            });

    public static Parser<string> Ignore(Regex parser) =>
        IPatterns.GetOrAdd(
            $"{parser}",
            (TextScanner scanner) =>
            {
                if (!scanner.StartsWith(parser, out var length))
                    return Expected.Regex<string>(parser, scanner.Offset);

                scanner.Offset += length;

                return new EmptyToken<string>(scanner.Offset - length);
            });

    public static Parser<T> Ignore<T>(Parser<T> parser) =>
        (TextScanner scanner) =>
        {
            var at = scanner.Offset;
            var result = parser(scanner);

            if (!result.IsToken)
                return Undo<T>(scanner, result.Offset, at, result.Conflict);

            return new EmptyToken<T>(at);
        };

    public static Parser<char> Atom(char c) =>
        Chars.GetOrAdd(
            c,
            (TextScanner scanner) =>
            {
                if (scanner.Offset == scanner.Chars.Length)
                    return DidNotExpect.EndOfText<char>(scanner.Offset);

                return scanner.StartsWith(c)
                    ? new Lexeme<char>(scanner.Offset, 1, scanner.ReadText().Span[0])
                    : Expected.Char<char>(c, scanner.Offset);
            });

    public static Parser<Characters> Atom(string parser) =>
        Strings.GetOrAdd(
            parser,
            (TextScanner scanner) =>
            {
                if ((scanner.Chars.Length - scanner.Offset) < parser.Length)
                    return DidNotExpect.EndOfText<Characters>(scanner.Offset);

                return scanner.StartsWith(parser)
                    ? new Lexeme<Characters>(scanner.Offset, parser.Length, scanner.ReadText(parser.Length))
                    : Expected.Text<Characters>(parser, scanner.Offset);
            });

    public static Parser<Characters> Atom(Regex parser) =>
        Patterns.GetOrAdd(
            $"{parser}",
            (TextScanner scanner) =>
                scanner.StartsWith(parser, out var length)
                    ? new Lexeme<Characters>(scanner.Offset, length, scanner.ReadText(length))
                    : Expected.Regex<Characters>(parser, scanner.Offset));

    public static IParseResult<char> StartOfText(TextScanner scanner)
    {
        if (scanner.Offset > 0)
            return Expected.StartOfText<char>(scanner.Offset);

        return new EmptyToken<char>(scanner.Offset);
    }
}