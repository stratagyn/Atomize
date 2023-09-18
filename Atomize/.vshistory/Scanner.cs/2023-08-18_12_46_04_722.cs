using System.Text.RegularExpressions;

namespace Atomize;

public record Scanner
{
    private readonly IDictionary<int, int>? _imap;

    public Scanner(string text, bool squeeze = false)
    {
        Offset = 0;
        
        if (squeeze)
            (Text, _imap = Squeeze(text));
        else
            Text = new ReadOnlyMemory<char>(text.ToCharArray());
    }

    internal Scanner(ReadOnlyMemory<char> text)
    {
        Offset = 0;
        Text = text;
    }

    public int Offset { get; internal set; }

    public int Remaining => Text.Length - Offset;

    public ReadOnlyMemory<char> Text { get; }

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

        var substring = Text.Slice(Offset, length);

        Offset += length;

        return substring;
    }

    public ReadOnlyMemory<char> ReadToEnd()
    {
        if (Offset == Text.Length)
            return ReadOnlyMemory<char>.Empty;

        var substring = Text[Offset..];

        Offset = Text.Length;

        return substring;
    }

    public void Reset() => Offset = 0;

    public bool StartsWith(char parser) =>
        Offset < Text.Length && Text.Span[Offset] == parser;

    public bool StartsWith(char[] parsers)
    {
        if (Remaining == 0)
            return false;

        var nextCharacter = Text.Span[Offset];

        foreach (var character in parsers)
            if (nextCharacter == character)
                return true;

        return false;
    }

    public bool StartsWith(string parser) =>
        Remaining >= parser.Length &&
        MemoryExtensions.Equals(
            Text.Slice(Offset, parser.Length).Span,
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
                    substring = Text.Slice(Offset, prefixLength);
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

        var matches = parser.EnumerateMatches(Text.Span, Offset);

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
            var matches = parser.EnumerateMatches(Text.Span, Offset);

            if (matches.MoveNext() && matches.Current.Index == Offset)
            {
                length = matches.Current.Length;

                return true;
            }
        }

        return false;
    }

    internal char ReadChar() => Text.Span[Offset++];

    internal ReadOnlyMemory<char> ReadText(int n = 1)
    {
        var substring = Text.Slice(Offset, n);

        Offset += n;

        return substring;
    }

    private (ReadOnlyMemory<char>, IDictionary<int, int>?) Squeeze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (ReadOnlyMemory<char>.Empty, null);

        static bool isEscaped(string input, int i) => input[i - 1] == '\\';

        var tokens = new List<char>(text.Length);
        var imap = new Dictionary<int, int>();
        var whitespace = 0;
        var stringContext = "";
        var index = 0;

        while (index < text.Length)
        {
            var match = Pattern.Text.Whitespace.Match(text, offset);

            if (match.Success && stringContext.Length == 0)
            {
                whitespace += match.Length;
                index += match.Length;
                continue;
            }

            imap[index - whitespace] = index;
            
            tokens.Add()
    }
}