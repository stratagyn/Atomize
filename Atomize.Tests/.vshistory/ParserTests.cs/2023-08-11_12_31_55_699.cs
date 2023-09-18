using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Parser;

namespace Atomize.Tests;

public static partial class AtomizeTests
{
    public class ChooseTests
    {
        private readonly TokenReader _reader;

        public ChooseTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Choose_Matching_EmptyRules_Is_EmptyToken()
        {
            var pattern = Array.Empty<Parser<ReadOnlyMemory<char>>>();
            var parsed = Choose(pattern)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Choose_Matching_Rule_Is_Token()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("abcd"), Literal("0123"), Literal("!@#$") };
            var parsed = Choose(pattern)(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Choose_Matching_Rule_Does_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("abcd"), Literal("0123"), Literal("!@#$") };
            var expected = _reader.Offset + 4;

            _ = Choose(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Choose_NonMatching_Rule_Is_Failure()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("ABCD"), Literal("0123"), Literal("!@#$") };
            var parsed = Choose(pattern)(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Choose_NonMatching_Rule_DoesNotFollowedBy_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("ABCD"), Literal("0123"), Literal("!@#$") };
            var expected = _reader.Offset;

            _ = Choose(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class EndOfTextTests
    {
        private readonly TokenReader _reader;

        public EndOfTextTests() => _reader = new TokenReader(TestText);

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
        private readonly TokenReader _reader;

        public ExactlyTests() => _reader = new TokenReader(TestText);

        
        [Fact]
        public void Exactly_Matching_Rule_Zero_Times_Is_EmptyToken()
        {
            var parsed = Exactly(0, Literal(Lowercase))(_reader);
            
            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Exactly_Matching_Rule_Exact_Count_Is_Token()
        {
            var parsed = Exactly(TestOffset, Literal(Lowercase))(_reader);
            
            Assert.True(parsed.IsToken);
            Assert.Equal(TestOffset, parsed.SemanticValue!.Count);
        }

        [Fact]
        public void Exactly_Matching_Rule_Exact_Count_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;
            var _ = Exactly(TestOffset, Literal(Lowercase))(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_Rule_LessThan_Count_Is_Failure()
        {
            var parsed = Exactly(TestOffset + 1, Literal(Lowercase))(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Exactly_Matching_Rule_LessThan_Count_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;
            var _ = Exactly(TestOffset + 1, Literal(Lowercase))(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_Rule_Negative_Count_Is_Failure()
        {
            var parsed = Exactly(-1, Literal(Lowercase))(_reader);

            Assert.False(parsed.IsToken);
        }
    }

    public class FallbackTests
    {
        private readonly TokenReader _reader;

        public FallbackTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Fallback_Empty_Is_EmptyToken()
        {
            var parsed = Fallback<char>()(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Fallback_Match_Is_Token()
        {
            var sequence = Fallback(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Fallback_Match_AdvancesOffset()
        {
            var sequence = Fallback(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            var expected = _reader.Offset + 4;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fallback_Match_NoRead_Is_Token()
        {
            var sequence = Fallback(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Fallback_Match_NoRead_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Fallback(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fallback_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = Fallback(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual)
        }

        [Fact]
        public void Fallback_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var sequence = Fallback(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var expected = _reader.Offset + 4;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fallback_StartingWith_NonMatch_Is_Failure()
        {
            var actual = Fallback(Literal('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Fallback_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Fallback(Literal('A'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fallback_NonMatch_AfterMatch_Is_Token()
        {
            var sequence = Fallback(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('0'));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Fallback_NonMatch_AfterMatch_AdvancesOffset_ByFirstMatch()
        {
            var sequence = Fallback(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('0'));

            var expected = _reader.Offset + 1;

            _ = sequence(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreTests
    {
        private readonly TokenReader _reader;

        public IgnoreTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Ignore_Matching_Rule_Is_Token()
        {
            var pattern = Literal("abcd");
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_Rule_DoesNotFollowedBy_AdvanceOffset()
        {
            var pattern = Literal("abcd");
            var expected = _reader.Offset + 4;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Rule_Is_Failure()
        {
            var pattern = Literal("0123");
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_Rule_DoesNotFollowedBy_AdvanceOffset()
        {
            var pattern = Literal("0123");
            var expected = _reader.Offset;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class InterleaveTests
    {
        private static readonly Parser<ReadOnlyMemory<char>>[] RuleTokens = new Parser<ReadOnlyMemory<char>>[] {
                Literal("foo"), Literal("bar"), Literal("baz"), Literal("qux")
            };

        [Fact]
        public void Interleave_Separator_EmptyRules_Is_EmptyToken()
        {
            var reader = new TokenReader("a b c d");
            var parse = Interleave(Literal(" "))(reader);
            
            Assert.True(parse.IsToken);
            Assert.Equal(0, parse.Length);
        }

        [Fact]
        public void Interleave_Space_MatchingRules_MatchingRule_Is_Token()
        {
            var reader = new TokenReader("foo bar baz qux");
            var parse = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Interleave_Space_MatchingRules_MatchingRule_Advances_Offset()
        {
            var reader = new TokenReader("foo bar baz qux");
            var expected = reader.Offset + 15;
            var _ = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Interleave_Space_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TokenReader("foo baz bar qux");
            var parse = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Interleave_Space_EndingWith_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TokenReader("foo bar baz quux");
            var parse = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Interleave_Space_MatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TokenReader("foobarbazqux");
            var parse = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Interleave_Space_NonMatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TokenReader("foobarbazquux");
            var parse = Interleave(Literal(" "), RuleTokens)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }
    }

    public class LongestPrefixTests
    {
        private readonly TokenReader _reader;

        public LongestPrefixTests() => _reader = new TokenReader(TestText);

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
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_Match_NoRead_Is_Token()
        {
            var sequence = LongestPrefix(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_NoRead_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = LongestPrefix(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void LongestPrefix_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = LongestPrefix(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_StartingWith_NonMatch_Is_Failure()
        {
            var actual = LongestPrefix(Literal('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void LongestPrefix_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Fallback(Literal('A'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LongestPrefix_NonMatch_Is_Token()
        {
            var sequence = LongestPrefix(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('0'));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);

            Assert.True(parse.Token?.GetType() == typeof(List<ParseResult>));
            Assert.True((char)((List<ParseResult>)parse.Token)[0].Token! == 'a');
            Assert.True((char)((List<ParseResult>)parse.Token)[1].Token! == 'b');
            Assert.True((char)((List<ParseResult>)parse.Token)[2].Token! == 'c');
        }

        [Fact]
        public void LongestPrefix_NonMatch_AdvancesOffset_ByLongestMatch()
        {
            var expected = _reader.Offset + 3;
            var sequence = LongestPrefix(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('0'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class MapTests
    {
        private static readonly Func<char, int> Char2Int = c => c;

        private readonly TokenReader _reader;

        public MapTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Map_Matching_Rule_Is_Token()
        {
            var expected = (int)'a';
            var parse = Map(Char2Int, Literal('a'))(_reader);
            var success = parse.IsToken;

            Assert.True(success);

            var actual = parse.SemanticValue;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Map_Matching_Rule_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Map(Char2Int, Literal('a'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Map_NonMatching_Rule_Is_Failure()
        {
            var actual = Map(Char2Int, Literal('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Map_NonMatching_Rule_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Map(Char2Int, Literal('A'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class PositiveLookAheadTests
    {
        private readonly TokenReader _reader;

        public PositiveLookAheadTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void IfFollowedBy_Matching_Rule_Is_Token()
        {
            var actual = IfFollowedBy(Literal("a"), Literal("b"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfFollowedBy_Matching_Rule_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = IfFollowedBy(Literal("a"), Literal("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Rule_Is_Failure()
        {
            var actual = IfFollowedBy(Literal("a"), Literal("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Rule_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Literal("a"), Literal("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfFollowedBy(Literal("A"), Literal("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Literal("A"), Literal("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class PositiveLookBehindTests
    {
        private readonly TokenReader _reader;

        public PositiveLookBehindTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void IfPrecededBy_Matching_Rule_Is_Token()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Literal("0123"), Literal(Lowercase))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfPrecededBy_Matching_Rule_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = IfPrecededBy(Literal("0123"), Literal(Lowercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Rule_Is_Failure()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Literal("0123"), Literal(Uppercase))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Rule_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = IfPrecededBy(Literal("0123"), Literal(Uppercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfPrecededBy(Literal("0123"), Literal(Uppercase))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfPrecededBy(Literal("0123"), Literal(Uppercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class NegativeLookBehindTests
    {
        private readonly TokenReader _reader;

        public NegativeLookBehindTests() => _reader = new TokenReader(TestText);


        [Fact]
        public void NotPrecededBy_Matching_Rule_Is_Failure()
        {
            _reader.Advance(TestOffset);

            var actual = NotPrecededBy(Literal("0123"), Literal(Lowercase))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_Matching_Rule_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = NotPrecededBy(Literal("0123"), Literal(Lowercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Rule_Is_Token()
        {
            _reader.Advance(TestOffset);

            var actual = NotPrecededBy(Literal("0123"), Literal(Uppercase))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Rule_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = NotPrecededBy(Literal("0123"), Literal(Uppercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotPrecededBy(Literal("0123"), Literal(Lowercase))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_DoesNotPrecededBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotPrecededBy(Literal("0123"), Literal(Lowercase));

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
    
    public class NegativeLookAheadTests
    {
        private readonly TokenReader _reader;

        public NegativeLookAheadTests() => _reader = new TokenReader(TestText);


        [Fact]
        public void NotFollowedBy_Matching_Rule_Is_Failure()
        {
            var actual = NotFollowedBy(Literal("a"), Literal("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_Matching_Rule_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Literal("a"), Literal("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Rule_Is_Token()
        {
            var actual = NotFollowedBy(Literal("a"), Literal("B"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Rule_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = NotFollowedBy(Literal("a"), Literal("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotFollowedBy(Literal("A"), Literal("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Literal("A"), Literal("B"));

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SequenceTests
    {
        private readonly TokenReader _reader;

        public SequenceTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Sequence_Empty_Is_Token()
        {
            var actual = Sequence()(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_Is_Token()
        {
            var sequence = Sequence(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('d'));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_Match_NoRead_Is_Token()
        {
            var sequence = Sequence(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var actual = sequence(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Sequence_Match_NoRead_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Ignore(Literal('a')),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_Match_ReadFirstMatchOnly_Is_Token()
        {
            var sequence = Sequence(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            var parse = sequence(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);

            Assert.True(parse.Token?.GetType() == typeof(char));
            Assert.True((char)parse.Token! == 'a');
        }

        [Fact]
        public void Sequence_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;
            var sequence = Sequence(
                Literal('a'),
                Ignore(Literal('b')),
                Ignore(Literal('c')),
                Ignore(Literal('d')));

            _ = sequence(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sequence_StartingWith_NonMatch_Is_Failure()
        {
            var actual = Sequence(Literal('A'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Sequence_NonMatch_Is_Failure()
        {
            var sequence = Sequence(
                Literal('a'),
                Literal('b'),
                Literal('c'),
                Literal('0'));

            var actual = sequence(_reader).IsToken;

            Assert.False(actual);
        }
    }

    

    

    public class SplitTests
    {
        [Fact]
        public void Split_Rule_MatchingRules_Is_Token()
        {
            var reader = new TokenReader("abba abba abba abba");
            var parse = Split(Literal(" "), Literal("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_Rule_MatchingRules_Advances_Offset()
        {
            var reader = new TokenReader("abba abba abba abba");
            var expected = reader.Offset + 19;
            _ = Split(Literal(" "), Literal("abba"))(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Split_Rule_NonMatchingRule_Is_Token()
        {
            var reader = new TokenReader("abba baab abba abba");
            var parse = Split(Literal(" "), Literal("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_Rule_StartingWith_NonMatchingRule_Is_Failure()
        {
            var reader = new TokenReader("baab abba abba abba");
            var parse = Split(Literal(" "), Literal("abba"))(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Split_Rule_NonMatchingSeparator_Rule_Is_Token()
        {
            var reader = new TokenReader("abbaabbaabbaabba");
            var parse = Split(Literal(" "), Literal("abba"))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }
    }

    public class StartOfTextTests
    {
        private readonly TokenReader _reader;

        public StartOfTextTests() => _reader = new TokenReader(TestText);

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

    public class ZeroOrMoreTests
    {
        private readonly TokenReader _reader;

        public ZeroOrMoreTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void ZeroOrMore_Matching_Rule_Zero_Times_Is_Token()
        {
            var actual = ZeroOrMore(Literal(Uppercase))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_Rule_Zero_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = ZeroOrMore(Literal(Uppercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_Rule_GreaterThan_Zero_Times_Is_Token()
        {
            var actual = ZeroOrMore(Literal(Lowercase))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_Rule_GreaterThan_Zero_Times_AdvancesOffset()
        {
 
            var expected = _reader.Offset + 26;

            _ = ZeroOrMore(Literal(Lowercase))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}
