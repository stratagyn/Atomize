using System.Text;
using System.Text.RegularExpressions;

namespace Atomize;

public record TextScanner
{
    private readonly IDictionary<int, int>? _imap;

    public TextScanner(string text, bool squeeze = false)
    {
        Text = text;
        Offset = 0;

        if (squeeze)
            (Chars, _imap) = Squeeze(text);
        else
            Chars = new ReadOnlyMemory<char>(text.ToCharArray());
    }

    internal TextScanner(ReadOnlyMemory<char> text)
    {
        Offset = 0;
        Chars = text;
    }

    internal ReadOnlyMemory<char> Chars { get; }

    public int Index => _imap is null ? Offset : _imap[Offset];

    public int Offset { get; internal set; }

    public int Remaining => Chars.Length - Offset;

    public string Text { get; }

    public void Advance(int count = 1)
    {
        if (count < 0 || count > Remaining)
            throw new InvalidOperationException($"Not enough text to advance. {Remaining} characters left.");

        Offset += count;
    }

    public void Backtrack(int count = 1)
    {
        if (count < 0 || count > Offset)
            throw new InvalidOperationException($"Not enough text to backtrack. Current offset is {Offset}.");

        Offset -= count;
    }

    public ReadOnlyMemory<char> Read(int length = 1)
    {
        if (length < 0 || length > Remaining)
            throw new InvalidOperationException($"Cannot read {length} characters.");

        var substring = Chars.Slice(Offset, length);

        Offset += length;

        return substring;
    }

    public ReadOnlyMemory<char> ReadToEnd()
    {
        if (Offset == Chars.Length)
            return ReadOnlyMemory<char>.Empty;

        var substring = Chars[Offset..];

        Offset = Chars.Length;

        return substring;
    }

    public void Reset() => Offset = 0;

    public bool StartsWith(char parser) =>
        Offset < Chars.Length && Chars.Span[Offset] == parser;

    public bool StartsWith(char[] parsers)
    {
        if (Remaining == 0)
            return false;

        var nextCharacter = Chars.Span[Offset];

        foreach (var character in parsers)
            if (nextCharacter == character)
                return true;

        return false;
    }

    public bool StartsWith(string parser) =>
        Remaining >= parser.Length &&
        MemoryExtensions.Equals(
            Chars.Slice(Offset, parser.Length).Span,
            parser,
            StringComparison.Ordinal);

    public bool StartsWith(string[] parsers) => StartsWith(parsers, out _);

    public bool StartsWith(string[] parsers, out string match)
    {
        match = "";

        if (Remaining == 0)
            return false;

        var prefixLength = -1;
        ReadOnlyMemory<char> substring = ReadOnlyMemory<char>.Empty;

        foreach (var parser in parsers)
        {
            if (Remaining >= parser.Length)
            {
                if (parser.Length != prefixLength)
                {
                    prefixLength = parser.Length;
                    substring = Chars.Slice(Offset, prefixLength);
                }

                if (substring.IsString(parser))
                {
                    match = parser;
                    return true;
                }
            }
        }

        return false;
    }

    public bool StartsWith(Regex parser) => StartsWith(parser, out _);

    public bool StartsWith(Regex parser, out int length)
    {
        length = 0;

        var matches = parser.EnumerateMatches(Chars.Span, Offset);

        if (matches.MoveNext() && matches.Current.Index == Offset)
        {
            length = matches.Current.Length;

            return true;
        }

        return false;
    }

    public bool StartsWith(Regex[] parsers, out int length)
    {
        length = 0;

        foreach (var parser in parsers)
        {
            var matches = parser.EnumerateMatches(Chars.Span, Offset);

            if (matches.MoveNext() && matches.Current.Index == Offset)
            {
                length = matches.Current.Length;

                return true;
            }
        }

        return false;
    }

    internal char ReadChar() => Chars.Span[Offset++];

    internal ReadOnlyMemory<char> ReadText(int n = 1)
    {
        var substring = Chars.Slice(Offset, n);

        Offset += n;

        return substring;
    }

    private static (ReadOnlyMemory<char>, IDictionary<int, int>?) Squeeze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (ReadOnlyMemory<char>.Empty, null);

        static bool isEscaped(string input, int i) => input[i - 1] == '\\';

        var tokens = new StringBuilder(text.Length);
        var imap = new Dictionary<int, int>();
        var whitespace = 0;
        var stringContext = 0;
        var index = 0;

        while (index < text.Length)
        {
            var match = Pattern.Text.Whitespace.Match(text, index);

            if (match.Success && match.Index == index && stringContext == 0)
            {
                whitespace += match.Length;
                index += match.Length;
                continue;
            }

            var c = text[index];

            imap[index - whitespace] = index;

            if (c == '\'' || c == '"')
                stringContext = stringContext == 0
                    ? c
                    : stringContext != c || isEscaped(text, index)
                        ? stringContext
                        : 0;

            tokens.Append(c);
            index++;
        }

        return (new ReadOnlyMemory<char>(tokens.ToString().ToCharArray()), imap);
    }
}