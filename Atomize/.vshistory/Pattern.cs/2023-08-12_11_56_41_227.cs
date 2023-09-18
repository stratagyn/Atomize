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

        public static readonly Regex Digit = DigitRegex();

        public static readonly Regex HexDigit = HexDigitRegex();

        public static readonly Regex Identifier = IdentifierRegex();

        public static readonly Regex Letter = LetterRegex();

        public static readonly Regex LowercaseIdentifier = LowercaseIdentifierRegex();

        public static readonly Regex LowercaseLetter = LowercaseLetterRegex();

        public static readonly Regex NonIdentifier = NonIdentifierRegex();

        public static readonly Regex Punctuation = PunctuationRegex();

        public static readonly Regex UppercaseIdentifier = UppercaseIdentifierRegex();

        public static readonly Regex UppercaseLetter = UppercaseLetterRegex();


        [GeneratedRegex(@"[a-zA-Z0-9]")]
        private static partial Regex AlphaNumericRegex();

        [GeneratedRegex(@".")]
        private static partial Regex AnyRegex();

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

        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UppercaseLetterRegex();

        

        
    }

    public static partial class Text
    {
        public static readonly Regex AlphaNumeric = AlphaNumericRegex();

        public static readonly Regex Any = AnyRegex();

        public static readonly Regex HexDigit = HexDigitsRegex();

        public static readonly Regex Integer = IntegerRegex();

        public static readonly Regex Identifier = IdentifierRegex();

        public static readonly Regex LowercaseIdentifier = LowercaseIdentifierRegex();

        public static readonly Regex LowercaseLetter = LowercaseLetterRegex();

        public static readonly Regex Letter = LetterRegex();

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
