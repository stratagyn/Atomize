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

    private static IParseResult<int> Additive(TokenReader reader) =>
        (_Additive ??= 
            Fallback(Multiplicative, Literal(PlusMinus).As(c => (int)c.Span[0]), Additive)
            .As(vars =>
            {
                var (x, y) = (vars[0], vars[2]);

                return vars[1] == '+' ? x + y : x - y;
            }))(reader);

    private static IParseResult<int> Multiplicative(TokenReader reader) =>
        (_Multiplicative ??=
            Fallback(Exponential, Literal(MulDiv).As(c => (int)c.Span[0]), Multiplicative)
            .As(vars =>
            {
                var (x, y) = (vars[0], vars[2]);

                return vars[1] == '*' ? x * y : x / y;
            }))(reader);

    private static IParseResult<int> Exponential(TokenReader reader) =>
        (_Exponential ??=
            Sequence(
                Atom, 
                ZeroOrMore(Sequence(Ignore(Literal('^').As(_ => 0)), Exponential))
                    .As(s => s[1])
                    .As(s =>
                    {
                        var pow = 
                        for (var i = s.Count - 1)
                    }))
                .As(vars =>
                {
                    var (x, y) = (vars[0], vars[2]);

                    return vars[1] == '*' ? x * y : x / y;
                }))(reader);

    private static Regex PlusMinus = PlusMinusRegex();

    private static Regex MulDiv = MulDivRegex();

    [GeneratedRegex("[+-]")]
    private static partial Regex PlusMinusRegex();

    [GeneratedRegex("[*/]")]
    private static partial Regex MulDivRegex();

}
