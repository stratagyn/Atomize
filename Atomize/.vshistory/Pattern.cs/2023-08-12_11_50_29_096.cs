using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Atomize;

public static class Pattern
{
    public static partial class Character
    {
        public static readonly Regex Any = AnyRegex();



        [GeneratedRegex(@".")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"\\|[!""#$%&'()*+,.\/:;<=>?@^_`{|}~-]")]
        private static partial Regex PunctuationRegex();
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

        public static readonly Regex Number = NumberRegex();

        public static readonly Regex OptionalWhitespace = OptionalWhitespaceRegex();

        public static readonly Regex Punctuation = PunctuationRegex();

        public static readonly Regex UppercaseIdentifier = UppercaseIdentifierRegex();

        public static readonly Regex UppercaseLetter = UppercaseLetterRegex();

        

        

        

        [GeneratedRegex(@".+")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"\s*")]
        private static partial Regex OptionalWhitespaceRegex();

        [GeneratedRegex(@"[!""#$%&'()*+,./:;<=>?@\^_`{|}~-]+")]
        private static partial Regex PunctuationRegex();
    }
}
