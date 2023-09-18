﻿using Newtonsoft.Json.Linq;
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

    public class IgnoreTests
    {
        private readonly TokenReader _reader;

        public IgnoreTests() => _reader = new TokenReader(TestText);

        [Fact]
        public void Ignore_Matching_Rule_Is_Successful()
        {
            var pattern = Literal("abcd");
            var actual = Ignore(pattern)(_reader).IsSuccess;

            Assert.True(actual);
        }

        [Fact]
        public void Ignore_Matching_Rule_DoesNot_AdvancesOffset()
        {
            var pattern = Literal("abcd");
            var expected = _reader.Offset + 4;

            _ = Ignore(pattern)(in _reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Ignore_NonMatching_Rule_Is_Failure()
        {
            var pattern = Literal("0123");
            var actual = Ignore(pattern)(_reader).IsSuccess;

            Assert.False(actual);
        }

        [Fact]
        public void Ignore_NonMatching_Rule_DoesNot_AdvancesOffset()
        {
            var pattern = Literal("0123");
            var expected = _reader.Offset;

            _ = Ignore(pattern)(in _reader);

            var actual = _reader.Offset;

            Assert.Equal(expected, actual);
        }
    }
}
