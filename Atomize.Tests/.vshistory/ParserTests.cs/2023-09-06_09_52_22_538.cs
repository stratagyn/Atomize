using static Atomize.Parse;

namespace Atomize.Tests;

public static partial class AtomizeTests
{
    public class BindTests
    {
        private readonly TextScanner _reader;

        public BindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Bind_Match_Is_Atom()
        {
            var parser = Bind(Atom('a'), a => Succeed(a));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Bind_Match_AdvancesOffset()
        {
            var parser = Bind(Atom('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;
            
            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Bind_Match_NoRead_Is_Atom()
        {
            var parser = Bind(Ignore('a'), a => Succeed(a));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Bind_Match_NoRead_AdvancesOffset()
        {
            var parser = Bind(Ignore('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Bind_Match_ReadFirstMatchOnly_Is_Atom()
        {
            var parser = Bind(Atom('a'), a => Bind(Ignore('b'), _ => Succeed(a)));
            var parse = parser(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Bind_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var parser = Bind(Atom('a'), a => Bind(Ignore('b'), _ => Succeed(a)));
            var expected = _reader.Offset + 2;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Bind_StartingWith_NonMatch_Is_Failure()
        {
            var actual = Bind<char, char>(Atom('A'), a => Empty<char>)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Bind_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Bind<char, char>(Atom('A'), a => Empty<char>)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Bind_NonMatch_AfterMatch_Is_Failure()
        {
            var parser = Bind(Atom('a'), a => Bind(Atom('B'), _ => Succeed(a)));
            var actual = parser(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Bind_NonMatch_AfterMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Bind(Atom('a'), a => Bind(Atom('B'), _ => Succeed(a)))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class ChoiceTests
    {
        private readonly TextScanner _reader;

        public ChoiceTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Choice_Matching_EmptyRules_Is_EmptyToken()
        {
            var pattern = Array.Empty<Parser<ReadOnlyMemory<char>>>();
            var parsed = Choice(pattern)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Choice_Matching_Is_Atom()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Atom("abcd"), Atom("0123"), Atom("!@#$") };
            var parsed = Choice(pattern)(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Choice_Matching_Does_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Atom("abcd"), Atom("0123"), Atom("!@#$") };
            var expected = _reader.Offset + 4;

            _ = Choice(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Choice_NonMatching_Is_Failure()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Atom("ABCD"), Atom("0123"), Atom("!@#$") };
            var parsed = Choice(pattern)(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Choice_NonMatching_DoesNotFollowedBy_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Atom("ABCD"), Atom("0123"), Atom("!@#$") };
            var expected = _reader.Offset;

            _ = Choice(pattern)(_reader);

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

            var parsed = EndOfText(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void EOT_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            _reader.Advance(TestText.Length);

            var expected = _reader.Offset;

            _ = EndOfText(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EOT_NotFollowedBy_AtEnd_Is_Failure()
        {
            var actual = EndOfText(_reader).IsToken;
            Assert.False(actual);
        }

        [Fact]
        public void EOT_NotFollowedBy_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = EndOfText(_reader);

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
            var parsed = Exactly(0, Parse.Character.LowercaseLetter)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Exactly_Matching_Exact_Count_Is_Atom()
        {
            var parsed = Exactly(TestOffset, Parse.Character.LowercaseLetter)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(TestOffset, parsed.Value!.Count);
        }

        [Fact]
        public void Exactly_Matching_Exact_Count_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;
            var _ = Exactly(TestOffset, Parse.Character.LowercaseLetter)(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_LessThan_Count_Is_Failure()
        {
            var parsed = Exactly(TestOffset + 1, Parse.Character.LowercaseLetter)(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Exactly_Matching_LessThan_Count_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;
            var _ = Exactly(TestOffset + 1, Parse.Character.LowercaseLetter)(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Exactly_Matching_Negative_Count_Is_Failure()
        {
            var parsed = Exactly(-1, Parse.Character.LowercaseLetter)(_reader);

            Assert.False(parsed.IsToken);
        }
    }

    public class HandleTests
    {
        private readonly TextScanner _reader;

        public HandleTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Handle_Match_Is_Atom()
        {
            var parser = Handle(Atom('a'), err => Empty<char>);
            var expected = 'a';
            var actual = parser(_reader).Value;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Handle_Match_AdvancesOffset()
        {
            var parser = Handle(Atom('a'), err => Empty<char>);
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Handle_NonMatch_To_Empty_Atom_Is_Atom()
        {
            var parser = Handle(Atom('A'), err => Empty<char>);
            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Handle_NonMatch_To_Empty_Atom_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Handle(Atom('A'), err => Empty<char>)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Handle_NonMatch_To_Match_Is_Atom()
        {
            var parser = Handle(Atom('A'), err => Atom('a'));
            var expected = 'a';
            var actual = parser(_reader).Value;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Handle_NonMatch_To_Match_Does_AdvanceOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Handle(Atom('A'), err => Atom('a'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreCharTests
    {
        private readonly TextScanner _reader;

        public IgnoreCharTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Atom()
        {
            var pattern = 'a';
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = 'a';
            var expected = _reader.Offset + 1;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = '0';
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = '0';
            var expected = _reader.Offset;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreStringTests
    {
        private readonly TextScanner _reader;

        public IgnoreStringTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Atom()
        {
            var pattern = "abcd";
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = "abcd";
            var expected = _reader.Offset + 4;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = "0123";
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = "0123";
            var expected = _reader.Offset;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreRegexTests
    {
        private readonly TextScanner _reader;

        public IgnoreRegexTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Atom()
        {
            var pattern = LowercaseLetter;
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = LowercaseLetter;
            var expected = _reader.Offset + 1;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = UppercaseLetter;
            var actual = Ignore(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = UppercaseLetter;
            var expected = _reader.Offset;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IgnoreParserTests
    {
        private readonly TextScanner _reader;

        public IgnoreParserTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Ignore_Matching_Is_Atom()
        {
            var pattern = Atom("abcd");
            var actual = Ignore<ReadOnlyMemory<char>>(pattern)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_DoesNot_AdvanceOffset()
        {
            var pattern = Atom("abcd");
            var expected = _reader.Offset + 4;

            _ = Ignore<ReadOnlyMemory<char>>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Is_Failure()
        {
            var pattern = Atom("0123");
            var actual = Ignore<ReadOnlyMemory<char>>(pattern)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_DoesNot_AdvanceOffset()
        {
            var pattern = Atom("0123");
            var expected = _reader.Offset;

            _ = Ignore<ReadOnlyMemory<char>>(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class IslandTests
    {
        [Fact]
        public void Island_Match_Is_Atom()
        {
            var reader = new TextScanner("(TEST)");

            var island = Island(
                Atom('('),
                Atom("TEST"),
                Atom(')'));

            var actual = island(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Island_Match_AdvancesOffset()
        {
            var reader = new TextScanner("(TEST)");

            var island = Island(
                Atom('('),
                Atom("TEST"),
                Atom(')'));

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
                Atom('('),
                Atom("TEST"),
                Atom(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("TEST)");

            var island = Island(
                Atom('('),
                Atom("TEST"),
            Atom(')'));

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
                Atom('('),
                Atom("TEST"),
                Atom(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_NonMatch_After_Open_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("(EST)");

            var island = Island(
                Atom('('),
                Atom("TEST"),
                Atom(')'));

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
                Atom('('),
                Atom("TEST"),
                Atom(')'));

            var actual = island(reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Island_EndingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var reader = new TextScanner("(TEST");

            var island = Island(
                Atom('('),
                Atom("TEST"),
            Atom(')'));

            var expected = reader.Offset;

            _ = island(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class JoinTests
    {
        private static readonly Parser<ReadOnlyMemory<char>>[] RuleAtoms = new Parser<ReadOnlyMemory<char>>[] {
                Atom("foo"), Atom("bar"), Atom("baz"), Atom("qux")
            };

        [Fact]
        public void Join_Separator_EmptyRules_Is_EmptyToken()
        {
            var reader = new TextScanner("a b c d");
            var parse = Join<ReadOnlyMemory<char>, int>(Atom(" "))(reader);

            Assert.True(parse.IsToken);
            Assert.Equal(0, parse.Length);
        }

        [Fact]
        public void Join_Space_MatchingRules_MatchingRule_Is_Atom()
        {
            var reader = new TextScanner("foo bar baz qux");
            var parse = Join(Atom(" "), RuleAtoms)(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Join_Space_MatchingRules_MatchingRule_Advances_Offset()
        {
            var reader = new TextScanner("foo bar baz qux");
            var expected = reader.Offset + 15;
            var _ = Join(Atom(" "), RuleAtoms)(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_Space_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foo baz bar qux");
            var parse = Join(Atom(" "), RuleAtoms)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_EndingWith_NonMatchingRules_MatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foo bar baz quux");
            var parse = Join(Atom(" "), RuleAtoms)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_MatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foobarbazqux");
            var parse = Join(Atom(" "), RuleAtoms)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Join_Space_NonMatchingRules_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("foobarbazquux");
            var parse = Join(Atom(" "), RuleAtoms)(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }
    }

    public class PositiveLookAheadTests
    {
        private readonly TextScanner _reader;

        public PositiveLookAheadTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void IfFollowedBy_Matching_Is_Atom()
        {
            var actual = IfFollowedBy(Atom("a"), Atom("b"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfFollowedBy_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = IfFollowedBy(Atom("a"), Atom("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Is_Failure()
        {
            var actual = IfFollowedBy(Atom("a"), Atom("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Atom("a"), Atom("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfFollowedBy(Atom("A"), Atom("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfFollowedBy(Atom("A"), Atom("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class PositiveLookBehindTests
    {
        private readonly TextScanner _reader;

        public PositiveLookBehindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void IfPrecededBy_Matching_Is_Atom()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void IfPrecededBy_Matching_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = IfPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Is_Failure()
        {
            _reader.Advance(TestOffset);

            var actual = IfPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = IfPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = IfPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void IfPrecededBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = IfPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class MapTests
    {
        private readonly TextScanner _reader;

        public MapTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Map_Match_Is_Atom()
        {
            var parser = Map(Atom('a'), a => (int)a);
            var expected = (int)'a';
            var actual = parser(_reader).Value;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Map_Match_AdvancesOffset()
        {
            var parser = Map(Atom('a'), a => (int)'a');
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Map_NonMatch_Is_Failure()
        {
            var parser = Map(Atom('A'), a =>(int)a);
            var actual = parser(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Map_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Map(Atom('a'), a => Map(Atom('A'), a => (int)a));

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
            var parsed = Maximum(0, Parse.Character.LowercaseLetter)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Maximum_Matching_Exactly_Max_Times_Is_Atom()
        {
            var actual = Maximum(1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Maximum_Matching_Exactly_Max_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Maximum(TestOffset, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Maximum_Matching_LessThan_Max_Times_Is_Atom()
        {
            var actual = Maximum(27, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Maximum_Matching_LessThan_Max_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Maximum(27, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Maximum_Matching_Negative_Max_Is_Failure()
        {
            var actual = Maximum(-1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Maximum_Matching_Negative_Max_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(-1, Parse.Character.LowercaseLetter)(_reader);

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
            var parsed = Minimum(0, Parse.Character.UppercaseLetter)(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void Minimum_Matching_Exactly_Min_Times_Is_Atom()
        {
            var actual = Minimum(1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Minimum_Matching_Exactly_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Minimum(TestOffset, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_LessThan_Min_Times_Is_Failure()
        {
            var actual = Minimum(27, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Minimum_Matching_LessThan_Min_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(27, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_GreaterThan_Min_Times_Is_Atom()
        {
            var actual = Minimum(25, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Minimum_Matching_GreaterThan_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Minimum(1, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Minimum_Matching_Negative_Min_Is_Failure()
        {
            var actual = Minimum(-1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Minimum_Matching_Negative_Min_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Minimum(-1, Parse.Character.LowercaseLetter)(_reader);

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

            var actual = NotPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_Matching_DoesNot_AdvanceOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset;

            _ = NotPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Is_Atom()
        {
            _reader.Advance(TestOffset);

            var actual = NotPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_AdvancesOffset()
        {
            _reader.Advance(TestOffset);

            var expected = _reader.Offset + 4;

            _ = NotPrecededBy(Atom("0123"), Parse.Character.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotPrecededBy_NonMatching_Start_DoesNotPrecededBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotPrecededBy(Atom("0123"), Parse.Character.LowercaseLetter);

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
            var actual = NotFollowedBy(Atom("a"), Atom("b"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_Matching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Atom("a"), Atom("b"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Is_Atom()
        {
            var actual = NotFollowedBy(Atom("a"), Atom("B"))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = NotFollowedBy(Atom("a"), Atom("B"))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_Is_Failure()
        {
            var actual = NotFollowedBy(Atom("A"), Atom("B"))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotFollowedBy_NonMatching_Start_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotFollowedBy(Atom("A"), Atom("B"));

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class NotExactlyTests
    {
        private readonly TextScanner _reader;

        public NotExactlyTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void NotExactly_Matching_LessThan_N_Times_Is_Atom()
        {
            var actual = NotExactly(2, Atom('a'))(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Matching_LessThan_N_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = NotExactly(2, Atom('a'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_Exactly_N_Times_Is_Failure()
        {
            var actual = NotExactly(TestOffset, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotExactly_Matching_Exactly_N_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotExactly(TestOffset, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_GreaterThan_N_Times_Is_Atom()
        {
            var actual = NotExactly(1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Matching_GreaterThan_N_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = NotExactly(1, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Full_Match_Is_Atom()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var actual = NotExactly(1, Parse.Character.LowercaseLetter)(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void NotExactly_Full_Match_AdvancesOffset()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var expected = reader.Offset + TestOffset;

            _ = NotExactly(1, Parse.Character.LowercaseLetter)(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExactly_Matching_Negative_N_Is_Failure()
        {
            var actual = NotExactly(-1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void NotExactly_Matching_Negative_N_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = NotExactly(-1, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class OptionalTests
    {
        private readonly TextScanner _reader;

        public OptionalTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Optional_Matching_Is_Atom()
        {
            var actual = Optional(Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Optional_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Optional(Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Optional_NonMatching_Is_Atom()
        {
            var actual = Optional(Parse.Character.UppercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Optional_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Optional(Parse.Character.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class OrTests
    {
        private readonly TextScanner _reader;

        public OrTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Or_Match_First_Is_Atom()
        {
            var parser = Or(Atom('a'), Atom('A'));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_First_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;
            var parser = Or(Atom('a'), Atom('A'));

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_Second_Is_Atom()
        {
            var parser = Or(Atom('A'), Atom('a'));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_Second_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;
            var parser = Or(Atom('A'), Atom('a'));

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_Both_Is_Atom()
        {
            var parser = Or(Atom('a'), Atom('b'));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Or_Match_Both_AdvancesOffset()
        {
            var expected = _reader.Offset + 2;
            var parser = Or(Atom('a'), Atom('b'));

            _ = parser(_reader);
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Or_Match_None_Is_Failure()
        {
            var actual = Or(Atom('A'), Atom('B'))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Or_Match_None_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Or(Atom('A'), Atom('B'))(_reader);

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
            var actual = Range(0, 0, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Range_Matching_AtLeast_Min_Times_Is_Atom()
        {
            var actual = Range(1, 26, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Range_Matching_AtLeast_Min_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Range(1, 27, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_LessThan_Min_Times_Is_Failure()
        {
            var actual = Range(27, 27, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_LessThan_Min_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(27, 27, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_Negative_Count_Is_Failure()
        {
            var actual = Range(-1, 26, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_Negative_Count_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(-1, 26, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Range_Matching_Max_LessThan_Min_Is_Failure()
        {
            var actual = Range(26, 1, Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Range_Matching_Max_LessThan_Min_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Range(26, 1, Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SatisfiesTests
    {
        private static readonly Predicate<char> CharIsA = c => c == 'a';
        private static readonly Predicate<char> CharIsB = c => c == 'b';

        private readonly TextScanner _reader;

        public SatisfiesTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Satisfies_Matching_Passes_Test_Is_Atom()
        {
            var parse = Satisfies(Atom('a'), CharIsA)(_reader);
            var success = parse.IsToken;

            Assert.True(success);
        }

        [Fact]
        public void Satisfies_Matching_Passes_Test_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Satisfies(Atom('a'), CharIsA)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Satisfies_Matching_Fails_Test_Is_Failure()
        {
            var actual = Satisfies(Atom('a'), CharIsB)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Satisfies_Matching_Fails_Test_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Satisfies(Atom('a'), CharIsB)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Satisfies_NonMatching_Is_Failure()
        {
            var actual = Satisfies(Atom('A'), CharIsA)(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Satisfies_NonMatching_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Satisfies(Atom('A'), CharIsA)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class SplitTests
    {
        [Fact]
        public void Split_MatchingRules_Is_Atom()
        {
            var reader = new TextScanner("abba abba abba abba");
            var parse = Split(Atom("abba"), Atom(" "))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_MatchingRules_Advances_Offset()
        {
            var reader = new TextScanner("abba abba abba abba");
            var expected = reader.Offset + 19;
            _ = Split(Atom("abba"), Atom(" "))(reader);
            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Split_NonMatchingRule_Is_Atom()
        {
            var reader = new TextScanner("abba baab abba abba");
            var parse = Split(Atom("abba"), Atom(" "))(reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Split_StartingWith_NonMatchingRule_Is_Failure()
        {
            var reader = new TextScanner("baab abba abba abba");
            var parse = Split(Atom("abba"), Atom(" "))(reader);
            var actual = parse.IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Split_NonMatchingSeparator_Is_Atom()
        {
            var reader = new TextScanner("abbaabbaabbaabba");
            var parse = Split(Atom("abba"), Atom(" "))(reader);
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
            var parsed = StartOfText(_reader);

            Assert.True(parsed.IsToken);
            Assert.Equal(0, parsed.Length);
        }

        [Fact]
        public void SOT_AtBeginning_DoesNotFollowedBy_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = StartOfText(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SOT_NotFollowedBy_AtBeginning_Is_Failure()
        {
            _reader.Advance();

            var actual = StartOfText(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void SOT_NotFollowedBy_AtEnd_DoesNotFollowedBy_AdvanceOffset()
        {
            _reader.Advance();

            var expected = _reader.Offset;

            _ = StartOfText(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class AtomTests
    {
        private readonly TextScanner _reader;

        public AtomTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Atom_Matching_Char_Is_Atom()
        {
            var actual = Atom('a')(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Atom_Matching_Char_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Atom('a')(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Atom_NonMatching_Char_Is_Failure()
        {
            var actual = Atom('0')(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Atom_NonMatching_Char_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Atom('0')(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Atom_Matching_String_Is_Atom()
        {
            var actual = Atom("abcd")(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Atom_Matching_String_AdvancesOffset()
        {
            var expected = _reader.Offset + 4;

            _ = Atom("abcd")(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Atom_NonMatching_String_Is_Failure()
        {
            var actual = Atom("0123")(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Atom_NonMatching_String_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Atom("0123")(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Atom_Matching_Regex_Is_Atom()
        {
            var actual = Parse.Character.LowercaseLetter(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void Atom_Matching_Regex_AdvancesOffset()
        {
            var expected = _reader.Offset + 1;

            _ = Parse.Character.LowercaseLetter(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Atom_NonMatching_Regex_Is_Failure()
        {
            var actual = Parse.Character.UppercaseLetter(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Atom_NonMatching_Regex_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = Parse.Character.UppercaseLetter(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class TryBindTests
    {
        private readonly TextScanner _reader;

        public TryBindTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void TryBind_Match_Is_Atom()
        {
            var parser = TryBind(Atom('a'), a => Succeed(a));

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_AdvancesOffset()
        {
            var parser = TryBind(Atom('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_Match_NoRead_Is_Atom()
        {
            var parser = TryBind(Ignore('a'), a => Succeed(a)); ;

            var actual = parser(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_NoRead_AdvancesOffset()
        {
            var parser = TryBind(Ignore('a'), a => Succeed(a));
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_Match_ReadFirstMatchOnly_Is_Atom()
        {
            var parser = TryBind(Atom('a'), a => TryBind(Ignore('b'), _ => Succeed(a)));

            var parse = parser(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_Match_ReadFirstMatchOnly_AdvancesOffset()
        {
            var parser = TryBind(Atom('a'), a => TryBind(Ignore('b'), _ => Succeed(a)));
            var expected = _reader.Offset + 2;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_StartingWith_NonMatch_Is_Failure()
        {
            var actual = TryBind(Atom('A'), a => Succeed(a))(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void TryBind_StartingWith_NonMatch_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = TryBind(Atom('A'), a => Succeed(a))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryBind_NonMatch_AfterMatch_Is_Atom()
        {
            var parser = TryBind(Atom('a'), a => Bind(Ignore('B'), _ => Succeed(a)));
            var parse = parser(_reader);
            var actual = parse.IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void TryBind_NonMatch_AfterMatch_AdvancesOffset_ByFirstMatch()
        {
            var parser = TryBind(Atom('a'), a => Bind(Ignore('B'), _ => Succeed(a)));
            var expected = _reader.Offset + 1;

            _ = parser(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class UntilTests
    {
        private readonly TextScanner _reader;

        public UntilTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void Until_Matching_Is_Atom()
        {
            var parsed = Until(Atom('0'))(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Until_Matching_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = Until(Atom('0'))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Until_NonMatching_Is_Atom()
        {
            var parsed = Until(Atom(' '))(_reader);

            Assert.True(parsed.IsToken);
        }

        [Fact]
        public void Until_NonMatching_AdvancesOffset()
        {
            var expected = _reader.Offset + _reader.Text.Length;

            _ = Until(Atom(' '))(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }

    public class ZeroOrMoreTests
    {
        private readonly TextScanner _reader;

        public ZeroOrMoreTests() => _reader = new TextScanner(TestText);

        [Fact]
        public void ZeroOrMore_Matching_Zero_Times_Is_Atom()
        {
            var actual = ZeroOrMore(Parse.Character.UppercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_Zero_Times_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = ZeroOrMore(Parse.Character.UppercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_GreaterThan_Zero_Times_Is_Atom()
        {
            var actual = ZeroOrMore(Parse.Character.LowercaseLetter)(_reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_Matching_GreaterThan_Zero_Times_AdvancesOffset()
        {
            var expected = _reader.Offset + TestOffset;

            _ = ZeroOrMore(Parse.Character.LowercaseLetter)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ZeroOrMore_FullMatch_Is_Atom()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var actual = ZeroOrMore(Parse.Character.LowercaseLetter)(reader).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void ZeroOrMore_FullMatch_AdvancesOffset()
        {
            var reader = new TextScanner(TestText[0..TestOffset]);
            var expected = reader.Offset + TestOffset;

            _ = ZeroOrMore(Parse.Character.LowercaseLetter)(reader);

            var actual = reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}