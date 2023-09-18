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
 * Exponential: Atom ('^' Exponential)*;
 * Atom: /[+-]?/ INTEGER | '(' Additive ')'
 */
public static partial class IntegerCalculator
{
    private static Parser<int> Atom =
        Optional(Literal(PlusMinusRegex()))
        .Then<ReadOnlyMemory<char>, int>(sign =>
                Choose(
                    Literal(Pattern.Text.Integer).As(n => int.Parse(n.Span)),
                    Island(Literal('('), Additive, Literal(')')))
                .As(n => sign.SemanticValue.Span[0] == '-' ? -n : n));


    private static Parser<int>? _Additive;
    private static Parser<int>? _Multiplicative;
    private static Parser<int>? _Exponential;

    private static Regex PlusMinus = PlusMinusRegex();

    private static Regex MulDiv = MulDivRegex();

    [GeneratedRegex("[+-]")]
    private static partial Regex PlusMinusRegex();

    [GeneratedRegex("[*/]")]
    private static partial Regex MulDivRegex();

}
