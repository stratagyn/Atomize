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

        [GeneratedRegex(@"\a")]
        private static partial Regex BellRegex();


    }

    public static partial class Text
    {
        public static readonly Regex Any = AnyRegex();

        public static readonly Regex OptionalWhitespace = OptionalWhitespaceRegex();

        public static readonly Regex Punctuation = PunctuationRegex();

        [GeneratedRegex(@".+")]
        private static partial Regex AnyRegex();

        [GeneratedRegex(@"\s*")]
        private static partial Regex OptionalWhitespaceRegex();

        [GeneratedRegex(@"[!""#$%&'()*+,./:;<=>?@\^_`{|}~-]+")]
        private static partial Regex PunctuationRegex();
    }
}
