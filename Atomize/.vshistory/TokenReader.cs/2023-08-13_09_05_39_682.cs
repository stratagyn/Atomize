using System.Text.RegularExpressions;

namespace Atomize;

public record TokenReader
{
    public TokenReader(string text, bool ignoreWhiteSpace = false)
    {
        IgnoreWhitespace = ignoreWhiteSpace;
        Offset = 0;
        Text = new ReadOnlyMemory<char>(text.ToCharArray());
    }

    internal TokenReader(ReadOnlyMemory<char> text)
    {
        Offset = 0;
        Text = text;
    }

    public bool IgnoreWhitespace { get; }

    public int Offset { get; internal set; }

    public int Remaining => Text.Length - Offset;

    public ReadOnlyMemory<char> Text { get; }

    public void Advance(int count = 1)
    {
        if (count < 0 || count > Remaining)
            throw new InvalidOperationException($"Not enough text to advance. {Remaining} characters left.");

        Offset += count;

        if (IgnoreWhitespace)
            while (char.IsWhiteSpace(Text.Span[Offset]))
                Offset++;
    }

    public void Backtrack(int count = 1)
    {
        if (count < 0 || count > Offset)
            throw new InvalidOperationException($"Not enough text to backtrack. Current offset is {Offset}.");

        Offset -= count;

        if (IgnoreWhitespace)
            while (char.IsWhiteSpace(Text.Span[Offset]))
                Offset--;
    }

    public char Read()
    {
        if (Offset == Text.Length)
            throw new InvalidOperationException("Reader is at end of text.");

        var next = Text.Span[Offset++];

        if (IgnoreWhitespace)
            while (char.IsWhiteSpace(Text.Span[Offset]))
                Offset++;

        return next;
    }

    public ReadOnlyMemory<char> Read(int length)
    {
        if (length < 0 || length > Remaining)
            throw new InvalidOperationException($"Cannot read {length} characters.");

        var substring = Text.Slice(Offset, length);

        Offset += length;

        if (IgnoreWhitespace)
            while (char.IsWhiteSpace(Text.Span[Offset]))
                Offset++;

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

    public bool StartsWith(char token) =>
        Offset < Text.Length && Text.Span[Offset] == token;

    public bool StartsWith(char[] tokens)
    {
        if (Remaining == 0)
            return false;

        var nextCharacter = Text.Span[Offset];

        foreach (var character in tokens)
            if (nextCharacter == character)
                return true;

        return false;
    }

    public bool StartsWith(string token) =>
        Remaining >= token.Length &&
        MemoryExtensions.Equals(
            Text.Slice(Offset, token.Length).Span,
            token,
            StringComparison.Ordinal);

    public bool StartsWith(string[] tokens) => StartsWith(tokens, out _);

    public bool StartsWith(string[] tokens, out string match)
    {
        match = "";

        if (Remaining == 0)
            return false;

        var prefixLength = -1;
        ReadOnlySpan<char> substring = "";

        foreach (var token in tokens)
        {
            if (Remaining >= token.Length)
            {
                if (token.Length != prefixLength)
                {
                    prefixLength = token.Length;
                    substring = Text.Slice(Offset, prefixLength).Span;
                }

                if (MemoryExtensions.Equals(substring, token, StringComparison.Ordinal))
                {
                    match = token;
                    return true;
                }
            }
        }

        return false;
    }

    public bool StartsWith(Regex token) => StartsWith(token, out _);

    public bool StartsWith(Regex token, out int length)
    {
        length = 0;

        var matches = token.EnumerateMatches(Text.Span, Offset);

        if (matches.MoveNext() && matches.Current.Index == Offset)
        {
            length = matches.Current.Length;

            return true;
        }

        return false;
    }

    public bool StartsWith(Regex[] tokens) => StartsWith(tokens, out _);

    public bool StartsWith(Regex[] tokens, out int length)
    {
        length = 0;

        foreach (var token in tokens)
        {
            var matches = token.EnumerateMatches(Text.Span, Offset);

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
}