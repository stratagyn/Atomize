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
        public void Choice_Matching_EmptyRules_Is_Successful()
        {
            var pattern = Array.Empty<ReadOnlyMemory<char>>();
            var actual = Choose(pattern)(_reader).IsSuccess;

            Assert.True(actual);
        }

        [Fact]
        public void Choice_Matching_Rule_Is_Successful()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("abcd"), Literal("0123"), Literal("!@#$") };
            var actual = Choice(pattern)(_reader).IsSuccess;

            Assert.True(actual);
        }

        [Fact]
        public void Choice_Matching_Rule_Does_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("abcd"), Literal("0123"), Literal("!@#$") };
            var expected = _reader.Offset + 4;

            _ = Choice(pattern)(in _reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Choice_NonMatching_Rule_Is_Failure()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("ABCD"), Literal("0123"), Literal("!@#$") };
            var actual = Choice(pattern)(_reader).IsSuccess;

            Assert.False(actual);
        }

        [Fact]
        public void Choice_NonMatching_Rule_DoesNot_AdvancesOffset()
        {
            var pattern = new Parser<ReadOnlyMemory<char>>[] { Literal("ABCD"), Literal("0123"), Literal("!@#$") };
            var expected = _reader.Offset;

            _ = Choice(pattern)(in _reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}
