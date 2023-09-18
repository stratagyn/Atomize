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
        public void Choose_NonMatching_Rule_DoesNot_AdvancesOffset()
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
        public void EOT_AtEnd_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _reader.Advance(TestText.Length);

            _ = EndOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EOT_Not_AtEnd_Is_Failure()
        {
            var actual = EndOfText<char>(_reader).IsToken;
            Assert.False(actual);
        }

        [Fact]
        public void EOT_Not_AtEnd_DoesNot_AdvanceOffset()
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
        public void Exactly_Matching_Rule_LessThan_Count_DoesNot_AdvanceOffset()
        {
            var parsed = Exactly(TestOffset + 1, Literal(Lowercase))(_reader);

            Assert.False(parsed.IsToken);
        }

        [Fact]
        public void Exactly_Matching_Rule_Negative_Count_Is_Failure()
        {
            var pattern = Token.Character(Character.LowercaseLetter);
            var actual = Exactly(pattern, -1)(_reader).IsToken;

            Assert.False(actual);
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
        public void Ignore_Matching_Rule_DoesNot_AdvanceOffset()
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
        public void Ignore_NonMatching_Rule_DoesNot_AdvanceOffset()
        {
            var pattern = Literal("0123");
            var expected = _reader.Offset;

            _ = Ignore(pattern)(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
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
        public void SOT_AtBeginning_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _ = StartOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SOT_Not_AtBeginning_Is_Failure()
        {
            _reader.Advance();

            var actual = StartOfText<char>(_reader).IsToken;

            Assert.False(actual);
        }

        [Fact]
        public void SOT_Not_AtEnd_DoesNot_AdvanceOffset()
        {
            var expected = _reader.Offset;

            _reader.Advance();

            _ = StartOfText<char>(_reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}
