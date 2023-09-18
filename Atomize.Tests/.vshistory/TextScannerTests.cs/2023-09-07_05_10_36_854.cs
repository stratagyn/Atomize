namespace Atomize.Tests;

public partial class AtomizeTests
{
    public class AtomReaderTests
    {
        private readonly TextScanner _scanner;

        public AtomReaderTests() => _scanner = new TextScanner(TestText);

        [Fact]
        public void NewAtomReader_Offset_Is_Zero()
        {
            var expected = 0;
            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Advance_Offset_IncreasesByOne()
        {
            var expected = _scanner.Offset + 1;

            _scanner.Advance();

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AdvanceAtEnd_Throws_InvalidOperationException()
        {
            _scanner.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _scanner.Advance());
        }

        [Fact]
        public void AdvanceN_Offset_IncreasesByN()
        {
            var expected = _scanner.Offset + TestOffset;

            _scanner.Advance(TestOffset);

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AdvancePastEnd_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Advance(TestText.Length + 1));

        [Fact]
        public void AdvanceNegative_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Advance(-1));

        [Fact]
        public void Backtrack_Offset_DecreasesByOne()
        {
            _scanner.Advance();

            var expected = _scanner.Offset - 1;

            _scanner.Backtrack();

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BacktrackPastBeginning_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Backtrack());

        [Fact]
        public void BacktrackN_Offset_DecreasesByN()
        {
            var expected = TestText.Length - TestOffset;

            _scanner.Advance(TestText.Length);
            _scanner.Backtrack(TestOffset);

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BacktrackAtBeginning_Throws_InvalidOperationException()
        {
            _scanner.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _scanner.Backtrack(TestText.Length + 1));
        }

        [Fact]
        public void BacktrackNegative_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Backtrack(-1));

        [Fact]
        public void Read_Is_CharacterAtCurrentOffset()
        {
            var expected = TestText[_scanner.Offset];
            var actual = _scanner.Read().Span[0];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Read_Offset_IncreasesByOne()
        {
            var expected = _scanner.Offset + 1;

            _ = _scanner.Read();

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadAtEnd_Throws_InvalidOperationException()
        {
            _scanner.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _scanner.Read());
        }

        [Fact]
        public void ReadN_Is_StringAtCurrentOffset()
        {
            var expected = TestText[0..TestOffset];
            var actual = _scanner.Read(TestOffset);

            Assert.True(MemoryExtensions.Equals(expected, actual.Span, StringComparison.Ordinal));
        }

        [Fact]
        public void ReadN_Offset_IncreasesByN()
        {
            var expected = _scanner.Offset + TestOffset;

            _ = _scanner.Read(TestOffset);

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadPastEnd_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Read(TestText.Length + 1));

        [Fact]
        public void ReadNegativeCount_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _scanner.Read(-1));

        [Fact]
        public void ReadToEnd_Is_StringFromCurrentOffset()
        {
            var expected = TestText;
            var actual = _scanner.ReadToEnd();

            Assert.True(MemoryExtensions.Equals(expected, actual.Span, StringComparison.Ordinal));
        }

        [Fact]
        public void ReadToEndAtEnd_Is_EmptyString()
        {
            _scanner.Advance(TestText.Length);

            var actual = _scanner.ReadToEnd();

            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void Reset_Offset_IsZero()
        {
            var expected = 0;

            _scanner.Advance(TestText.Length);

            var advanced = _scanner.Offset > 0;

            Assert.True(advanced);

            _scanner.Reset();

            var actual = _scanner.Offset;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void StartsWith_Character_Matches_ExpectedOutcome(int index, bool expected)
        {
            var actual = _scanner.StartsWith(TestText[index]);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartsWith_Character_AtEnd_Is_False()
        {
            _scanner.Advance(TestText.Length);

            var actual = _scanner.StartsWith(TestText[0]);

            Assert.False(actual);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void StartsWith_String_AtOffset_Matches_ExpectedOutcome(int index, bool expected)
        {
            var actual = _scanner.StartsWith(TestText[index..TestOffset]);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartsWith_String_AtEnd_Is_False()
        {
            _scanner.Advance(TestText.Length);

            var actual = _scanner.StartsWith(TestText[0]);

            Assert.False(actual);
        }

        [Fact]
        public void StartsWith_Matching_Regex_Is_True()
        {
            var actual = Parse.Character.LowercaseLetter(_scanner).IsToken;

            Assert.True(actual);
        }

        [Fact]
        public void StartsWith_Matching_Regex_AtDifferentOffset_Is_False()
        {
            _scanner.Advance(TestOffset);

            var actual = Parse.Character.LowercaseLetter(_scanner).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void StartsWith_NonMatching_Regex_Is_False()
        {
            var actual = Parse.Text.Number(_scanner).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void Squeeze_Unquoted_Text_Reduces_Size()
        {
            var text = "0 1 2 3 4 5 6 7 8 9";
            var expected = 10;
            var scanner = new TextScanner(text, true);
            var actual = scanner.Length;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Squeeze_Quoted_Text_DoesNot_Reduce_Size()
        {
            var text = "'0 1 2 3 4 5 6 7 8 9'";
            var expected = text.Length;
            var scanner = new TextScanner(text, true);
            var actual = scanner.Length;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Squeeze_Unquoted_And_Quoted_Text_Reduces_Size()
        {
            var quoted = "\"0 1 2 3 4 5 6 7 8 9\"";
            var text = $"0 1 2 3 4 5 6 7 8 9 {quoted}";
            var expected = 10 + quoted.Length;
            var scanner = new TextScanner(text, true);
            var actual = scanner.Length;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Squeeze_EscapedQuoted_Text_Reduces_Size()
        {
            var text = "\\'0 1 2 3 4 5 6 7 8 9\\'";
            var expected = 14;
            var scanner = new TextScanner(text, true);
            var actual = scanner.Length;

            Assert.Equal(expected, actual);
        }
    }
}