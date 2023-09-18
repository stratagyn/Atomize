using System.Text.RegularExpressions;

namespace Atomize.Tests;

public partial class AtomizeTests
{
    public class TokenReaderTests
    {
        private readonly TokenReader _reader;

        public TokenReaderTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void NewTokenReader_Offset_Is_Zero()
        {
            var expected = 0;
            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NewTokenReader_Remaining_Is_LengthOfInput()
        {
            var expected = TestText.Length;
            var actual = _reader.Remaining;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Advance_Offset_IncreasesByOne()
        {
            var expected = _reader.Offset + 1;

            _reader.Advance();

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AdvanceAtEnd_Throws_InvalidOperationException()
        {
            _reader.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _reader.Advance());
        }

        [Fact]
        public void AdvanceN_Offset_IncreasesByN()
        {
            var expected = _reader.Offset + TestOffset;

            _reader.Advance(TestOffset);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AdvancePastEnd_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Advance(TestText.Length + 1));

        [Fact]
        public void AdvanceNegative_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Advance(-1));

        [Fact]
        public void Backtrack_Offset_DecreasesByOne()
        {
            _reader.Advance();

            var expected = _reader.Offset - 1;

            _reader.Backtrack();

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BacktrackPastBeginning_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Backtrack());

        [Fact]
        public void BacktrackN_Offset_DecreasesByN()
        {
            var expected = TestText.Length - TestOffset;

            _reader.Advance(TestText.Length);
            _reader.Backtrack(TestOffset);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BacktrackAtBeginning_Throws_InvalidOperationException()
        {
            _reader.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _reader.Backtrack(TestText.Length + 1));
        }

        [Fact]
        public void BacktrackNegative_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Backtrack(-1));

        [Fact]
        public void Read_Is_CharacterAtCurrentOffset()
        {
            var expected = TestText[_reader.Offset];
            var actual = _reader.Read().Span[0];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Read_Offset_IncreasesByOne()
        {
            var expected = _reader.Offset + 1;

            _ = _reader.Read();

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadAtEnd_Throws_InvalidOperationException()
        {
            _reader.Advance(TestText.Length);

            Assert.Throws<InvalidOperationException>(() => _reader.Read());
        }

        [Fact]
        public void ReadN_Is_StringAtCurrentOffset()
        {
            var expected = TestText[0..TestOffset];
            var actual = _reader.Read(TestOffset);

            Assert.True(MemoryExtensions.Equals(expected, actual.Span, StringComparison.Ordinal));
        }

        [Fact]
        public void ReadN_Offset_IncreasesByN()
        {
            var expected = _reader.Offset + TestOffset;

            _ = _reader.Read(TestOffset);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadPastEnd_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Read(TestText.Length + 1));

        [Fact]
        public void ReadNegativeCount_Throws_InvalidOperationException() =>
            Assert.Throws<InvalidOperationException>(() => _reader.Read(-1));

        [Fact]
        public void ReadToEnd_Is_StringFromCurrentOffset()
        {
            var expected = TestText;
            var actual = _reader.ReadToEnd();

            Assert.True(MemoryExtensions.Equals(expected, actual.Span, StringComparison.Ordinal));
        }

        [Fact]
        public void ReadToEndAtEnd_Is_EmptyString()
        {
            _reader.Advance(TestText.Length);

            var actual = _reader.ReadToEnd();

            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void Reset_Offset_IsZero()
        {
            var expected = 0;

            _reader.Advance(TestText.Length);

            var advanced = _reader.Offset > 0;

            Assert.True(advanced);

            _reader.Reset();

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void StartsWith_Character_Matches_ExpectedOutcome(int index, bool expected)
        {
            var actual = _reader.StartsWith(TestText[index]);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartsWith_Character_AtEnd_Is_False()
        {
            _reader.Advance(TestText.Length);

            var actual = _reader.StartsWith(TestText[0]);

            Assert.False(actual);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void StartsWith_String_AtOffset_Matches_ExpectedOutcome(int index, bool expected)
        {
            var actual = _reader.StartsWith(TestText[index..TestOffset]);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartsWith_String_AtEnd_Is_False()
        {
            _reader.Advance(TestText.Length);

            var actual = _reader.StartsWith(TestText[0]);

            Assert.False(actual);
        }


        [Fact]
        public void StartsWith_Matching_Regex_Is_True()
        {
            var actual = _reader.StartsWith(new Regex(@"[a-z]"));

            Assert.True(actual);
        }

        [Fact]
        public void StartsWith_Matching_Regex_AtDifferentOffset_Is_False()
        {
            _reader.Advance(TestOffset);

            var actual = _reader.StartsWith(Lowercase);

            Assert.False(actual);
        }

        [Fact]
        public void StartsWith_NonMatching_Regex_Is_False()
        {
            var actual = _reader.StartsWith(Number);

            Assert.False(actual);
        }
    }
}