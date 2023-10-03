using System.Text.RegularExpressions;

namespace Atomize;

public static partial class Parse
{
   public static partial class Multiple
   {
      public static readonly Parser<ReadOnlyMemory<char>> AlphaNumeric = Atom(AlphaNumericRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Any = Atom(AnyRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Digit = Atom(DigitRegex());

      public static readonly Parser<ReadOnlyMemory<char>> DoubleQuotedString = Atom(DoubleQuotedStringRegex());

      public static readonly Parser<ReadOnlyMemory<char>> EscapeSequence = Atom(EscapeSequenceRegex());

      public static readonly Parser<ReadOnlyMemory<char>> HexDigit = Atom(HexDigitRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Identifier = Atom(IdentifierRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Letter = Atom(LetterRegex());

      public static readonly Parser<ReadOnlyMemory<char>> LowercaseIdentifier = Atom(LowercaseIdentifierRegex());

      public static readonly Parser<ReadOnlyMemory<char>> LowercaseLetter = Atom(LowercaseLetterRegex());

      public static readonly Parser<ReadOnlyMemory<char>> NewLine = Atom(NewLineRegex());

      public static readonly Parser<ReadOnlyMemory<char>> NonIdentifier = Atom(NonIdentifierRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Number = Atom(NumberRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Punctuation = Atom(PunctuationRegex());

      public static readonly Parser<ReadOnlyMemory<char>> QuotedString = Atom(QuotedStringRegex());

      public static readonly Parser<ReadOnlyMemory<char>> RegularExpression = Atom(RegularExpressionRegex());

      public static readonly Parser<ReadOnlyMemory<char>> SingleQuotedString = Atom(SingleQuotedStringRegex());

      public static readonly Parser<ReadOnlyMemory<char>> Unicode = Atom(UnicodeRegex());

      public static readonly Parser<ReadOnlyMemory<char>> UppercaseIdentifier = Atom(UppercaseIdentifierRegex());

      public static readonly Parser<ReadOnlyMemory<char>> UppercaseLetter = Atom(UppercaseLetterRegex());

      internal static readonly Regex WhitespacePattern = WhitespaceRegex();

      public static readonly Parser<ReadOnlyMemory<char>> Whitespace = Atom(WhitespacePattern);

      [GeneratedRegex(@"[a-zA-Z0-9]+")]
      private static partial Regex AlphaNumericRegex();

      [GeneratedRegex(@".+")]
      private static partial Regex AnyRegex();

      [GeneratedRegex(@"[0-9]+")]
      private static partial Regex DigitRegex();

      [GeneratedRegex(@"""(.*(?<!\\))""")]
      private static partial Regex DoubleQuotedStringRegex();

      [GeneratedRegex(@"(\\[\\'0abfnrtv""])+")]
      private static partial Regex EscapeSequenceRegex();

      [GeneratedRegex(@"[0-9a-fA-Fa-f]+")]
      private static partial Regex HexDigitRegex();

      [GeneratedRegex(@"[A-Za-z_][A-Za-z0-9_]*")]
      private static partial Regex IdentifierRegex();

      [GeneratedRegex(@"[a-z0-9_]+")]
      private static partial Regex LowercaseIdentifierRegex();

      [GeneratedRegex(@"[a-z]+")]
      private static partial Regex LowercaseLetterRegex();

      [GeneratedRegex(@"[a-zA-Z]+")]
      private static partial Regex LetterRegex();

      [GeneratedRegex(@"(\u000D\u000A?|\u000A|\u0085|\u2028|\u2029)+")]
      private static partial Regex NewLineRegex();

      [GeneratedRegex(@"[^a-zA-Z0-9_]+")]
      private static partial Regex NonIdentifierRegex();

      [GeneratedRegex(@"([0-9]*\.[0-9]+)|([0-9][0-9]*)")]
      private static partial Regex NumberRegex();

      [GeneratedRegex(@"(\\|[!""#$%&'()*+,.\/:;<=>?@^_`{|}~-])+")]
      private static partial Regex PunctuationRegex();

      [GeneratedRegex(@"(""(.*(?<!\\))"")|('(.*(?<!\\))')")]
      private static partial Regex QuotedStringRegex();

      [GeneratedRegex(@"/(.*?(?<!\\))/")]
      private static partial Regex RegularExpressionRegex();

      [GeneratedRegex(@"'(.*(?<!\\))'")]
      private static partial Regex SingleQuotedStringRegex();

      [GeneratedRegex(@"(\\u(?:([0-9a-fA-F]{4})|(?:\{((?:10[0-9a-fA-F]{4})|(?:0?[0-9a-fA-F]{5})|(?:[0-9a-fA-F]{1,4}))\})))+")]
      private static partial Regex UnicodeRegex();

      [GeneratedRegex(@"[A-Z0-9_]+")]
      private static partial Regex UppercaseIdentifierRegex();

      [GeneratedRegex(@"[A-Z]+")]
      private static partial Regex UppercaseLetterRegex();

      [GeneratedRegex(@"(\u0009|\u000B|\u000C|\u0020|\u00A0|\u1680|\u180E|[\u2000-\u200A]|\u202F|\u3000|\u205F|\u000D\u000A?|\u000A|\u0085|\u2028|\u2029)+")]
      private static partial Regex WhitespaceRegex();
   }
}