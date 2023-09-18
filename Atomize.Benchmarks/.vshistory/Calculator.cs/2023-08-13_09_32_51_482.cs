using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Atomize.Parser;

namespace Atomize.Benchmarks;

/*
 * Additive: Multiplicative /[+-]/ Additive | Multiplicative;
 * Multiplicative: Exponential /[*\/]/ Multiplicative | Exponential;
 * Exponential: Atom ('^' Expoential)*;
 * Atom: INTEGER | '(' Additive ')'
 */
public static partial class IntegerCalculator
{
    private static Parser<int> Atom =
        Optional(Choose(Literal('-'), Literal('+')))
            .Then(sign => 
            );

    [GeneratedRegex("[+-]")]
    private static partial Regex PlusMinusRegex();

    [GeneratedRegex("[*/%]")]
    private static partial Regex MulRegex();

}
