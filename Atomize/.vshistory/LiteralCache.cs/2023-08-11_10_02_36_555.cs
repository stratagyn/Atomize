using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Atomize.Failure;

namespace Atomize;

internal class LiteralCache
{
    private static readonly ConcurrentDictionary<char, Parser<char>> Characters =
        new();

    private static readonly ConcurrentDictionary<string, Parser<ReadOnlyMemory<char>>> Strings =
        new();

    private static readonly ConcurrentDictionary<string, Parser<ReadOnlyMemory<char>>> Patterns =
        new();

    public Parser<char> Match(char c) =>
        Characters.GetOrAdd(
            c,
            (TokenReader reader) =>
            {
                if (reader.Remaining == 0)
                    return DidNotExpect.EndOfText<char>(reader.Offset);

                return reader.StartsWith(token)
                    ? new Character(reader.Offset, reader.ReadChar())
                    : Expected.Char<char>(token, reader.Offset);
            });
}
