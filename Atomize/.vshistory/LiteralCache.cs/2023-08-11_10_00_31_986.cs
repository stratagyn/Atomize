using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize;

internal class LiteralCache
{
    private static readonly ConcurrentDictionary<char, Parser<char>> Characters =
        new ConcurrentDictionary<char, Parser<char>>();

    private static readonly ConcurrentDictionary<string, Parser<ReadOnlyMemory<char>>> Strings =
        new();
}
