using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Atomize;

public static partial class Pattern
{
    public static partial class Character
    {
        public static readonly Regex AlphaNumeric = AlphaNumericRegex();

        public static readonly Regex Any = AnyRegex();

        public static readonly Regex Control = ControlRegex();

        public static readonly Regex Digit = DigitRegex();

        public static readonly Regex Escaped = EscapedRegex();

        public static readonly Regex EscapedHex = EscapedHexRegex();

        public static readonly Regex EscapedUnicode = EscapedUnicodeRegex();    

        public static readonly Regex HexDigit = HexDigitRegex();

        public static readonly Regex Identifier = IdentifierRegex();

        public static readonly Regex Letter = LetterRegex();

        public static readonly Regex LowercaseIdentifier = LowercaseIdentifierRegex();

        public static readonly Regex LowercaseLetter = LowercaseLetterRegex();

        public static readonly Regex NewLine = NewLineRegex();

        public static readonly Regex NonIdentifier = NonIdentifierRegex();

        public static readonly Regex Punctuation = PunctuationRegex();

        public static readonly Regex UppercaseIdentifier = UppercaseIdentifierRegex();

        public static readonly Regex UppercaseLetter = UppercaseLetterRegex();

        public static readonly Regex Whitespace = WhitespaceRegex();


        [GeneratedRegex(@"[a-zA-Z0-9]")]
        private static partial Regex AlphaNumericRegex();

        [GeneratedRegex(@".")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"[\x00-\x1F\x7F]")]
        private static partial Regex ControlRegex();

        [GeneratedRegex(@"\\[\\'0abfnrtv""]")]
        private static partial Regex EscapedRegex();

        [GeneratedRegex(@"\\x[0-9a-fA-F]{1,4}")]
        private static partial Regex EscapedHexRegex();

        [GeneratedRegex(@"(\\u[0-9a-fA-F]{4}])|(\\U[0-9a-fA-F]{8})")]
        private static partial Regex EscapedUnicodeRegex();

        [GeneratedRegex(@"\r|\n|\u0085|\u2028|\u2029")]
        private static partial Regex NewLineRegex();

        [GeneratedRegex(@"[0-9]")]
        private static partial Regex DigitRegex();

        [GeneratedRegex(@"[0-9A-F]")]
        private static partial Regex HexDigitRegex();

        [GeneratedRegex(@"[a-zA-Z0-9_]")]
        private static partial Regex IdentifierRegex();

        [GeneratedRegex(@"[a-zA-Z]")]
        private static partial Regex LetterRegex();

        [GeneratedRegex(@"[a-z0-9_]")]
        private static partial Regex LowercaseIdentifierRegex();

        [GeneratedRegex(@"[a-z]")]
        private static partial Regex LowercaseLetterRegex();

        [GeneratedRegex(@"[^a-zA-Z0-9_]")]
        private static partial Regex NonIdentifierRegex();

        [GeneratedRegex(@"\\|[!""#$%&'()*+,.\/:;<=>?@^_`{|}~-]")]
        private static partial Regex PunctuationRegex();

        [GeneratedRegex(@"[A-Z0-9_]")]
        private static partial Regex UppercaseIdentifierRegex();

        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UppercaseLetterRegex();

        [GeneratedRegex(@"\u0009|\u000B|\u000C|\u0020|\u00A0|\u1680|\u180E|[\u2000-\u200A]|\u202F|\u3000|\u205F")]
        private static partial Regex WhitespaceRegex();
    }

    public static partial class Text
    {
        public static readonly Regex AlphaNumeric = AlphaNumericRegex();

        public static readonly Regex Any = AnyRegex();

        public static readonly Regex HexDigit = HexDigitsRegex();

        public static readonly Regex Integer = IntegerRegex();

        public static readonly Regex Identifier = IdentifierRegex();

        public static readonly Regex Letter = LetterRegex();

        public static readonly Regex LowercaseIdentifier = LowercaseIdentifierRegex();

        public static readonly Regex LowercaseLetter = LowercaseLetterRegex();

        public static readonly Regex NonIdentifier = NonIdentifierRegex();

        public static readonly Regex Number = NumberRegex();

        public static readonly Regex Punctuation = PunctuationRegex();

        public static readonly Regex UppercaseIdentifier = UppercaseIdentifierRegex();

        public static readonly Regex UppercaseLetter = UppercaseLetterRegex();

        public static readonly Regex Whitespace = WhitespaceRegex();

        [GeneratedRegex(@"[a-zA-Z0-9]+")]
        private static partial Regex AlphaNumericRegex();

        [GeneratedRegex(@".+")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"[0-9A-F]+")]
        private static partial Regex HexDigitsRegex();

        [GeneratedRegex(@"[A-Za-z_][A-Za-z0-9]*")]
        private static partial Regex IdentifierRegex();

        [GeneratedRegex(@"[0-9][0-9]*")]
        private static partial Regex IntegerRegex();

        [GeneratedRegex(@"[a-z0-9_]+")]
        private static partial Regex LowercaseIdentifierRegex();

        [GeneratedRegex(@"[a-z]+")]
        private static partial Regex LowercaseLetterRegex();

        [GeneratedRegex(@"[a-zA-Z]+")]
        private static partial Regex LetterRegex();

        [GeneratedRegex(@"[^a-zA-Z0-9_]+")]
        private static partial Regex NonIdentifierRegex();

        [GeneratedRegex(@"([0-9]*\.[0-9]+)|([0-9][0-9]*)")]
        private static partial Regex NumberRegex();

        [GeneratedRegex(@"(\\|[!""#$%&'()*+,.\/:;<=>?@^_`{|}~-])+")]
        private static partial Regex PunctuationRegex();

        [GeneratedRegex(@"[A-Z0-9_]+")]
        private static partial Regex UppercaseIdentifierRegex();

        [GeneratedRegex(@"[A-Z]+")]
        private static partial Regex UppercaseLetterRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
