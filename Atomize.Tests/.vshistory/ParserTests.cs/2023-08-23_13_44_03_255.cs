using static Atomize.Parser;

namespace Atomize.Tests;

public static partial class AtomizeTests
{
    public class ChooseTests
    {
        private readonly TextScanner _reader;

        public ChooseTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Choose_Matching_EmptyRules_Is_EmptyToken()
        {
            var pattern = Array.Empty<Parser<ReadOnlyMemory<char>>>();
            var parsed = Choose(pattern)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Choose_Matching_Is_Token()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Token("abcd"), Token("0123"), Token("!@#$") };
            var parsed = Choose(pattern)(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Choose_Matching_Does_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Token("abcd"), Token("0123"), Token("!@#$") };
            var expected = _reader.Offset + 4;

            _ = Choose(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Choose_NonMatching_Is_Failure()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Token("ABCD"), Token("0123"), Token("!@#$") };
            var parsed = Choose(pattern)(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Choose_NonMatching_DoesNotFollowedBy_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Token("ABCD"), Token("0123"), Token("!@#$") };
            var expected = _reader.Offset;

            _ = Choose(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class EndOfTextTests
    {
        private readonly TextScanner _reader;

        public EndOfTextTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void EOT_AtEnd_Is_EmptyToken()
        {
            _reader.Advance(TestText.Length);

            var parsed = EndOfText<char>(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void EOT_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            _reader.Advance(TestText.Length);

            var expected = _reader.Offset;

            _ = EndOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EOT_NotFollowedBy_AtEnd_Is_Failure()
        {
            var actual = EndOfText<char>(_reader).IsToken;
            Assert.False(actual);
        }

        [Fact]
        public void EOT_NotFollowedBy_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = EndOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class ExactlyTests
    {
        private readonly TextScanner _reader;

        public ExactlyTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Exactly_Matching_Zero_Times_Is_EmptyToken()
        {
            var parsed = Exactly(0, Token(Pattern.Char.LowercaseLetter))(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Exactly_Matching_Exact_Count_Is_Token()
        {
            var parsed = Exactly(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(TestOffset, parsed.Value!.Count);
        }

        [Fact]
        public void Exactly_Matching_Exact_Count_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;
            var _ = Exactly(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_LessThan_Count_Is_Failure()
        {
            var parsed = Exactly(TestOffset + 1, Token(Pattern.Char.LowercaseLetter))(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Exactly_Matching_LessThan_Count_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;
            var _ = Exactly(TestOffset + 1, Token(Pattern.Char.LowercaseLetter))(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_Negative_Count_Is_Failure()
        {
            var parsed = Exactly(-1, Token(Pattern.Char.LowercaseLetter))(_reader);

            Assert.False(parsed.IsToken);
        }
    }

    public class TryBindTests
    {
        private readonly TextScanner _reader;

        public TryBindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void TryBind_Match_Is_Token()
        {
            var sequence = TryBind(Token('a'), a => Succeed(a));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_AdvancesOffset()
        {
            var sequence = TryBind(Token('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_Match_NoRead_Is_Token()
        {
            var sequence = TryBind(Ignore('a'), a => Succeed(a)); ;

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_NoRead_AdvancesOffset()
        {
            var sequence = TryBind(Ignore('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;
            
            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = TryBind(Token('a'), a => TryBind(Ignore('b'), _ => Succeed(a)));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var sequence = TryBind(Token('a'), a => TryBind(Ignore('b'), _ => Succeed(a)));
            var expected = _reader.Offset + 2;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_StartingWith_NonMatch_Is_Failure()
        {
            var actual = TryBind(Token('A'), a => Succeed(a))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void TryBind_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = TryBind(Token('A'), a => Succeed(a))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_NonMatch_AfterMatch_Is_Token()
        {
            var sequence = TryBind(Token('a'), a => Bind(Ignore('B'), _ => Succeed(a)));
            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_NonMatch_AfterMatch_AdvancesOffset_ByFirstMatch()
        {
            var sequence = TryBind(Token('a'), a => Bind(Ignore('B'), _ => Succeed(a)));
            var expected = _reader.Offset + 1;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreCharTests
    {
        private readonly TextScanner _reader;

        public IgnoreCharTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Token()
        {
            var pattern = 'a';
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = 'a';
            var expected = _reader.Offset + 1;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = '0';
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = '0';
            var expected = _reader.Offset;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreStringTests
    {
        private readonly TextScanner _reader;

        public IgnoreStringTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Token()
        {
            var pattern = "abcd";
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = "abcd";
            var expected = _reader.Offset + 4;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = "0123";
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = "0123";
            var expected = _reader.Offset;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreRegexTests
    {
        private readonly TextScanner _reader;

        public IgnoreRegexTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Token()
        {
            var pattern = Pattern.Char.LowercaseLetter;
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = Pattern.Char.LowercaseLetter;
            var expected = _reader.Offset + 1;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = Pattern.Char.UppercaseLetter;
            var actual = Ignore<char>(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = Pattern.Char.UppercaseLetter;
            var expected = _reader.Offset;

            _ = Ignore<char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreParserTests
    {
        private readonly TextScanner _reader;

        public IgnoreParserTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Token()
        {
            var pattern = Token("abcd");
            var actual = Ignore<ReadOnlyMemory<char>, char>(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = Token("abcd");
            var expected = _reader.Offset + 4;

            _ = Ignore<ReadOnlyMemory<char>, char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = Token("0123");
            var actual = Ignore<ReadOnlyMemory<char>, char>(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = Token("0123");
            var expected = _reader.Offset;

            _ = Ignore<ReadOnlyMemory<char>, char>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IslandTests
    {
        [Fact]
        public void Island_Match_Is_Token()
        {
            var reader = new TextScanner("(TEST)");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var actual = island(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Island_Match_AdvancesOffset()
        {
            var reader = new TextScanner("(TEST)");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var expected = reader.Offset + 6;

            _ = island(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Island_StartingWith_NonMatch_Is_Failure()
        {
            var reader = new TextScanner("TEST)");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("TEST)");

            var island = Island(
                Token('('),
                Token("TEST"),
            Token(')'));

            var expected = reader.Offset;

            _ = island(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Island_NonMatch_After_Open_Is_Failure()
        {
            var reader = new TextScanner("(EST)");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_NonMatch_After_Open_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("(EST)");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var expected = reader.Offset;

            _ = island(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Island_EndingWith_NonMatch_Is_Failure()
        {
            var reader = new TextScanner("(TEST");

            var island = Island(
                Token('('),
                Token("TEST"),
                Token(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_EndingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("(TEST");

            var island = Island(
                Token('('),
                Token("TEST"),
            Token(')'));

            var expected = reader.Offset;

            _ = island(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class JoinTests
    {
        private static readonly Parser<ReadOnlyMemory<char>>[] RuleTokens = new Parser<ReadOnlyMemory<char>>[] {
                Token("foo"), Token("bar"), Token("baz"), Token("qux")
            };

        [Fact]
        public void Join_Separator_EmptyRules_Is_EmptyToken()
        {
            var reader = new TextScanner("a b c d");
            var parse = Join<ReadOnlyMemory<char>, int>(Token(" "))(reader);

            Assert.True(parse.IsToken);
            Assert.Equal(0, parse.Length);
        }

        [Fact]
        public void Join_Space_MatchingRules_MatchingRule_Is_Token()
        {
            var reader = new TextScanner("foo bar baz qux");
            var parse = Join(Token(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Join_Space_MatchingRules_MatchingRule_Advances_Offset()
        {
            var reader = new TextScanner("foo bar baz qux");
            var expected = reader.Offset + 15;
            var _ = Join(Token(" "), RuleTokens)(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_Space_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foo baz bar qux");
            var parse = Join(Token(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_EndingWith_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foo bar baz quux");
            var parse = Join(Token(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_MatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foobarbazqux");
            var parse = Join(Token(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_NonMatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foobarbazquux");
            var parse = Join(Token(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }
    }

    public class TokenTests
    {
        private readonly TextScanner _reader;

        public TokenTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Token_Matching_Char_Is_Token()
        {
            var actual = Token('a')(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Token_Matching_Char_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Token('a')(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Token_NonMatching_Char_Is_Failure()
        {
            var actual = Token('0')(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Token_NonMatching_Char_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Token('0')(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Token_Matching_String_Is_Token()
        {
            var actual = Token("abcd")(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Token_Matching_String_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;

            _ = Token("abcd")(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Token_NonMatching_String_Is_Failure()
        {
            var actual = Token("0123")(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Token_NonMatching_String_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Token("0123")(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Token_Matching_Regex_Is_Token()
        {
            var actual = Token(Pattern.Char.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Token_Matching_Regex_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Token(Pattern.Char.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Token_NonMatching_Regex_Is_Failure()
        {
            var actual = Token(Pattern.Char.UppercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Token_NonMatching_Regex_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Token(Pattern.Char.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class LongestPrefixTests
    {
        private readonly TextScanner _reader;

        public LongestPrefixTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void LongestPrefix_Empty_Is_EmptyToken()
        {
            var parsed = LongestPrefix<char>()(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void LongestPrefix_Match_Is_Token()
        {
            var sequence = LongestPrefix(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('d'));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('d'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_Match_NoRead_Is_Token()
        {
            var sequence = LongestPrefix(
                Ignore<ReadOnlyMemory<char>, char>(Token('a')),
                Ignore<ReadOnlyMemory<char>, char>(Token('b')),
                Ignore<ReadOnlyMemory<char>, char>(Token('c')),
                Ignore<ReadOnlyMemory<char>, char>(Token('d')));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_NoRead_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Ignore<ReadOnlyMemory<char>, char>(Token('a')),
                Ignore<ReadOnlyMemory<char>, char>(Token('b')),
                Ignore<ReadOnlyMemory<char>, char>(Token('c')),
                Ignore<ReadOnlyMemory<char>, char>(Token('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = LongestPrefix(
                Token('a'),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('b')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('c')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('d')));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Token('a'),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('b')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('c')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_StartingWith_NonMatch_Is_Failure()
        {
            var actual = LongestPrefix(Token('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void LongestPrefix_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = LongestPrefix(Token('A'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_NonMatch_Is_Token()
        {
            var sequence = LongestPrefix(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('0'));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_NonMatch_AdvancesOffset_ByLongestMatch()
        {
            var expected = _reader.Offset + 3;

            var sequence = LongestPrefix(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('0'));

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class PositiveLookAheadTests
    {
        private readonly TextScanner _reader;

        public PositiveLookAheadTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void IfFollowedBy_Matching_Is_Token()
        {
            var actual = IfFollowedBy(Token("a"), Token("b"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfFollowedBy_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = IfFollowedBy(Token("a"), Token("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Is_Failure()
        {
            var actual = IfFollowedBy(Token("a"), Token("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Token("a"), Token("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfFollowedBy(Token("A"), Token("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Token("A"), Token("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class PositiveLookBehindTests
    {
        private readonly TextScanner _reader;

        public PositiveLookBehindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void IfPrecededBy_Matching_Is_Token()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfPrecededBy_Matching_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = IfPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Is_Failure()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = IfPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class MaximumTests
    {
        private readonly TextScanner _reader;

        public MaximumTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Maximum_Matching_Zero_Times_Is_EmptyToken()
        {
            var parsed = Maximum(0, Token(Pattern.Char.LowercaseLetter))(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Maximum_Matching_Exactly_Max_Times_Is_Token()
        {
            var actual = Maximum(1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Maximum_Matching_Exactly_Max_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Maximum(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Maximum_Matching_LessThan_Max_Times_Is_Token()
        {
            var actual = Maximum(27, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Maximum_Matching_LessThan_Max_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Maximum(27, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Maximum_Matching_Negative_Max_Is_Failure()
        {
            var actual = Maximum(-1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Maximum_Matching_Negative_Max_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(-1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class MinimumTests
    {
        private readonly TextScanner _reader;

        public MinimumTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Minimum_Matching_Zero_Times_Is_EmptyToken()
        {
            var parsed = Minimum(0, Token(Pattern.Char.UppercaseLetter))(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Minimum_Matching_Exactly_Min_Times_Is_Token()
        {
            var actual = Minimum(1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Minimum_Matching_Exactly_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Minimum(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_LessThan_Min_Times_Is_Failure()
        {
            var actual = Minimum(27, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Minimum_Matching_LessThan_Min_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(27, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_GreaterThan_Min_Times_Is_Token()
        {
            var actual = Minimum(25, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Minimum_Matching_GreaterThan_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Minimum(1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_Negative_Min_Is_Failure()
        {
            var actual = Minimum(-1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Minimum_Matching_Negative_Min_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(-1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class NegativeLookBehindTests
    {
        private readonly TextScanner _reader;

        public NegativeLookBehindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void NotPrecededBy_Matching_Is_Failure()
        {
            _reader.Advance(TestOffset);

            var actual = NotPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_Matching_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = NotPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Is_Token()
        {
            _reader.Advance(TestOffset);

            var actual = NotPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = NotPrecededBy(Token("0123"), Token(Pattern.Char.UppercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_DoesNotPrecededBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotPrecededBy(Token("0123"), Token(Pattern.Char.LowercaseLetter));

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class NegativeLookAheadTests
    {
        private readonly TextScanner _reader;

        public NegativeLookAheadTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void NotFollowedBy_Matching_Is_Failure()
        {
            var actual = NotFollowedBy(Token("a"), Token("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_Matching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Token("a"), Token("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Is_Token()
        {
            var actual = NotFollowedBy(Token("a"), Token("B"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = NotFollowedBy(Token("a"), Token("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotFollowedBy(Token("A"), Token("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Token("A"), Token("B"));

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class NotExactlyTests
    {
        private readonly TextScanner _reader;

        public NotExactlyTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void NotExactly_Matching_LessThan_N_Times_Is_Token()
        {
            var actual = NotExactly(2, Token('a'))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Matching_LessThan_N_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = NotExactly(2, Token('a'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_Exactly_N_Times_Is_Failure()
        {
            var actual = NotExactly(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotExactly_Matching_Exactly_N_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotExactly(TestOffset, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_GreaterThan_N_Times_Is_Token()
        {
            var actual = NotExactly(1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Matching_GreaterThan_N_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = NotExactly(1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Full_Match_Is_Token()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var actual = NotExactly(1, Token(Pattern.Char.LowercaseLetter))(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Full_Match_AdvancesOffset()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var expected = reader.Offset + TestOffset;

            _ = NotExactly(1, Token(Pattern.Char.LowercaseLetter))(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_Negative_N_Is_Failure()
        {
            var actual = NotExactly(-1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotExactly_Matching_Negative_N_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotExactly(-1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class OptionalTests
    {
        private readonly TextScanner _reader;

        public OptionalTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Optional_Matching_Is_Token()
        {
            var actual = Optional(Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Optional_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Optional(Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Optional_NonMatching_Is_Token()
        {
            var actual = Optional(Token(Pattern.Char.UppercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Optional_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Optional(Token(Pattern.Char.UppercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class OrTests
    {
        private readonly TextScanner _reader;

        public OrTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Or_Match_First_Is_Token()
        {
            var sequence = Or(Token('a'), Token('A'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_First_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;
            var sequence = Or(Token('a'), Token('A'));

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_Second_Is_Token()
        {
            var sequence = Or(Token('A'), Token('a'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_Second_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;
            var sequence = Or(Token('A'), Token('a'));

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_Both_Is_Token()
        {
            var sequence = Or(Token('a'), Token('b'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_Both_AdvancesOffset()
        {
            var expected = _reader.Offset + 2;
            var sequence = Or(Token('a'), Token('b'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_None_Is_Failure()
        {
            var actual = Or(Token('A'), Token('B'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Or_Match_None_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Or(Token('A'), Token('B'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class RangeTests
    {
        private readonly TextScanner _reader;

        public RangeTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Range_Matching_Zero_Times_Is_EmptyToken()
        {
            var actual = Range(0, 0, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Range_Matching_AtLeast_Min_Times_Is_Token()
        {
            var actual = Range(1, 26, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Range_Matching_AtLeast_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Range(1, 27, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_LessThan_Min_Times_Is_Failure()
        {
            var actual = Range(27, 27, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_LessThan_Min_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(27, 27, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_Negative_Count_Is_Failure()
        {
            var actual = Range(-1, 26, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_Negative_Count_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(-1, 26, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_Max_LessThan_Min_Is_Failure()
        {
            var actual = Range(26, 1, Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_Max_LessThan_Min_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(26, 1, Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SatisfiesTests
    {
        private static readonly Predicate<ReadOnlyMemory<char>> CharIsA = c => c.Span[0] == 'a';
        private static readonly Predicate<ReadOnlyMemory<char>> CharIsB = c => c.Span[0] == 'b';

        private readonly TextScanner _reader;

        public SatisfiesTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Satisfies_Matching_Passes_Test_Is_Token()
        {
            var parse = Satisfies(Token('a'), CharIsA)(_reader);
            var success = parse.IsToken;

            Assert.True(success);
        }

        [Fact]
        public void Satisfies_Matching_Passes_Test_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Satisfies(Token('a'), CharIsA)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Satisfies_Matching_Fails_Test_Is_Failure()
        {
            var actual = Satisfies(Token('a'), CharIsB)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Satisfies_Matching_Fails_Test_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Satisfies(Token('a'), CharIsB)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Satisfies_NonMatching_Is_Failure()
        {
            var actual = Satisfies(Token('A'), CharIsA)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Satisfies_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Satisfies(Token('A'), CharIsA)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SequenceTests
    {
        private readonly TextScanner _reader;

        public SequenceTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Sequence_Empty_Is_EmptyToken()
        {
            var parsed = Sequence<char>()(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Sequence_Match_Is_Token()
        {
            var sequence = Sequence(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('d'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('d'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_Match_NoRead_Is_Token()
        {
            var sequence = Sequence(
                Ignore<ReadOnlyMemory<char>, char>(Token('a')),
                Ignore<ReadOnlyMemory<char>, char>(Token('b')),
                Ignore<ReadOnlyMemory<char>, char>(Token('c')),
                Ignore<ReadOnlyMemory<char>, char>(Token('d')));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_NoRead_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Ignore<ReadOnlyMemory<char>, char>(Token('a')),
                Ignore<ReadOnlyMemory<char>, char>(Token('b')),
                Ignore<ReadOnlyMemory<char>, char>(Token('c')),
                Ignore<ReadOnlyMemory<char>, char>(Token('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = Sequence(
                Token('a'),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('b')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('c')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('d')));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Token('a'),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('b')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('c')),
                Ignore<ReadOnlyMemory<char>, ReadOnlyMemory<char>>(Token('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_StartingWith_NonMatch_Is_Failure()
        {
            var actual = Sequence(Token('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Sequence_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Sequence(Token('A'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_NonMatch_AfterMatch_Is_Failure()
        {
            var sequence = Sequence(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('0'));

            var actual = sequence(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void TryBind_NonMatch_AfterMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Sequence(
                Token('a'),
                Token('b'),
                Token('c'),
                Token('0'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SplitTests
    {
        [Fact]
        public void Split_MatchingRules_Is_Token()
        {
            var reader = new TextScanner("abba abba abba abba");
            var parse = Split(Token(" "), Token("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_MatchingRules_Advances_Offset()
        {
            var reader = new TextScanner("abba abba abba abba");
            var expected = reader.Offset + 19;
            _ = Split(Token(" "), Token("abba"))(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Split_NonMatchingRule_Is_Token()
        {
            var reader = new TextScanner("abba baab abba abba");
            var parse = Split(Token(" "), Token("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_StartingWith_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("baab abba abba abba");
            var parse = Split(Token(" "), Token("abba"))(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Split_NonMatchingSeparator_Is_Token()
        {
            var reader = new TextScanner("abbaabbaabbaabba");
            var parse = Split(Token(" "), Token("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }
    }

    public class StartOfTextTests
    {
        private readonly TextScanner _reader;

        public StartOfTextTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void SOT_AtBeginning_Is_EmptyToken()
        {
            var parsed = StartOfText<char>(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void SOT_AtBeginning_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = StartOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SOT_NotFollowedBy_AtBeginning_Is_Failure()
        {
            _reader.Advance();

            var actual = StartOfText<char>(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void SOT_NotFollowedBy_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            _reader.Advance();

            var expected = _reader.Offset;

            _ = StartOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class UntilTests
    {
        private readonly TextScanner _reader;

        public UntilTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Until_Matching_Is_Token()
        {
            var parsed = Until(Token('0'))(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Until_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Until(Token('0'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Until_NonMatching_Is_Token()
        {
            var parsed = Until(Token(' '))(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Until_NonMatching_AdvancesOffset()
        {
            var expected = _reader.Offset + _reader.Chars.Length;

            _ = Until(Token(' '))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class ZeroOrMoreTests
    {
        private readonly TextScanner _reader;

        public ZeroOrMoreTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void ZeroOrMore_Matching_Zero_Times_Is_Token()
        {
            var actual = ZeroOrMore(Token(Pattern.Char.UppercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_Zero_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = ZeroOrMore(Token(Pattern.Char.UppercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_GreaterThan_Zero_Times_Is_Token()
        {
            var actual = ZeroOrMore(Token(Pattern.Char.LowercaseLetter))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_GreaterThan_Zero_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = ZeroOrMore(Token(Pattern.Char.LowercaseLetter))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ZeroOrMore_FullMatch_Is_Token()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var actual = ZeroOrMore(Token(Pattern.Char.LowercaseLetter))(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_FullMatch_AdvancesOffset()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var expected = reader.Offset + TestOffset;

            _ = ZeroOrMore(Token(Pattern.Char.LowercaseLetter))(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}