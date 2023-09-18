using System.Text.RegularExpressions;

namespace Atomize;

public static partial class Parse
{
    public static partial class Character
    {
        public static readonly Parser<char> AlphaNumeric = Map(
            Atom(AlphaNumericRegex()), token => token.Span[0]);

        public static readonly Parser<char> Any = Map(
            Atom(AnyRegex()), token => token.Span[0]);

        public static readonly Parser<char> Control = Map(
            Atom(ControlRegex()), token => token.Span[0]);

        public static readonly Parser<char> Digit = Map(
            Atom(DigitRegex()), token => token.Span[0]);

        public static readonly Parser<char> EscapeSequence = Map(
            Atom(EscapeSequenceRegex()), token => token.Span[0]);

        public static readonly Parser<char> EscapedHex = Map(
            Atom(EscapedHexRegex()), token => token.Span[0]);

        public static readonly Parser<char> EscapedUnicode = Map(
            Atom(EscapedUnicodeRegex()), token => token.Span[0]);

        public static readonly Parser<char> HexDigit = Map(
            Atom(HexDigitRegex()), token => token.Span[0]);

        public static readonly Parser<char> Identifier = Map(
            Atom(IdentifierRegex()), token => token.Span[0]);

        public static readonly Parser<char> Letter = Map(
            Atom(LetterRegex()), token => token.Span[0]);

        public static readonly Parser<char> LowercaseIdentifier = Map(
            Atom(LowercaseIdentifierRegex()), token => token.Span[0]);

        public static readonly Parser<char> LowercaseLetter = Map(
            Atom(LowercaseLetterRegex()), token => token.Span[0]);

        public static readonly Parser<char> NewLine = Map(
            Atom(NewLineRegex()), token => token.Span[0]);

        public static readonly Parser<char> NonIdentifier = Map(
            Atom(NonIdentifierRegex()), token => token.Span[0]);

        public static readonly Parser<char> Punctuation = Map(
            Atom(PunctuationRegex()), token => token.Span[0]);

        public static readonly Parser<char> UppercaseIdentifier = Map(
            Atom(UppercaseIdentifierRegex()), token => token.Span[0]);

        public static readonly Parser<char> UppercaseLetter = Map(
            Atom(UppercaseLetterRegex()), token => token.Span[0]);

        public static readonly Parser<char> Whitespace = Map(
            Atom(WhitespaceRegex()), token => token.Span[0]);

        [GeneratedRegex(@"[a-zA-Z0-9]")]
        private static partial Regex AlphaNumericRegex();

        [GeneratedRegex(@".")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"[\x00-\x1F\x7F]")]
        private static partial Regex ControlRegex();

        [GeneratedRegex(@"\\[\\'0abfnrtv""]")]
        private static partial Regex EscapeSequenceRegex();

        [GeneratedRegex(@"\\x[0-9a-fA-F]{1,4}")]
        private static partial Regex EscapedHexRegex();

        [GeneratedRegex(@"(\\u[0-9a-fA-F]{4}])|(\\U[0-9a-fA-F]{8})")]
        private static partial Regex EscapedUnicodeRegex();

        [GeneratedRegex(@"[0-9]")]
        private static partial Regex DigitRegex();

        [GeneratedRegex(@"[0-9a-fA-Fa-f]")]
        private static partial Regex HexDigitRegex();

        [GeneratedRegex(@"[a-zA-Z0-9_]")]
        private static partial Regex IdentifierRegex();

        [GeneratedRegex(@"[a-zA-Z]")]
        private static partial Regex LetterRegex();

        [GeneratedRegex(@"[a-z0-9_]")]
        private static partial Regex LowercaseIdentifierRegex();

        [GeneratedRegex(@"[a-z]")]
        private static partial Regex LowercaseLetterRegex();

        [GeneratedRegex(@"\u000D\u000A?|\u000A|\u0085|\u2028|\u2029")]
        private static partial Regex NewLineRegex();

        [GeneratedRegex(@"[^a-zA-Z0-9_]")]
        private static partial Regex NonIdentifierRegex();

        [GeneratedRegex(@"\\|[!""#$%&'()*+,.\/:;<=>?@^_`{|}~-]")]
        private static partial Regex PunctuationRegex();

        [GeneratedRegex(@"[A-Z0-9_]")]
        private static partial Regex UppercaseIdentifierRegex();

        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UppercaseLetterRegex();

        [GeneratedRegex(@"\u0009|\u000B|\u000C|\u0020|\u00A0|\u1680|\u180E|[\u2000-\u200A]|\u202F|\u3000|\u205F|\u000D\u000A?|\u000A|\u0085|\u2028|\u2029")]
        private static partial Regex WhitespaceRegex();
    }
}