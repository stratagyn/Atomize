using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Parser;

namespace Atomize.Benchmarks;

public static class Calculator
{
    private static Parser<int> Atom = Sequence(Optional(Literal( )))
}
