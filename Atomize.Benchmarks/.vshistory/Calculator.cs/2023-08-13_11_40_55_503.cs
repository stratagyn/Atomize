﻿using System;
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
 * Atom: /[+-]?/ NUMBER | '(' Additive ')'
 */
public static partial class IntegerCalculator
{
    private static Parser<double> Atom =
        Optional(Literal(PlusMinusRegex()))
        .Then(sign =>
                Choose(
                    Literal(Pattern.Text.Integer).As(n => double.Parse(n.Span)),
                    Island(Literal('('), Additive, Literal(')')))
                .As(n => sign.SemanticValue.Length > 0 && sign.SemanticValue.Span[0] == '-' ? -n : n));


    private static Parser<double>? _Additive;
    private static Parser<double>? _Multiplicative;
    private static Parser<double>? _Exponential;

    public static IParseResult<double> Parse(string expression) => Additive(new(expression));

    private static IParseResult<double> Additive(TokenReader reader) =>
        (_Additive ??= 
            Fallback(Multiplicative, Literal(PlusMinus).As(c => (double)(int)c.Span[0]), Additive)
            .As(vars =>
            {
                if (vars.Count == 1)
                    return vars[0];

                var (x, y) = (vars[0], vars[2]);

                return vars[1] == '+' ? x + y : x - y;
            }))(reader);

    private static IParseResult<double> Multiplicative(TokenReader reader) =>
        (_Multiplicative ??=
            Fallback(Exponential, Literal(MulDiv).As(c => (double)(int)c.Span[0]), Multiplicative)
            .As(vars =>
            {
                if (vars.Count == 1)
                    return vars[0];

                var (x, y) = (vars[0], vars[2]);

                return vars[1] == '*' ? x * y : x / y;
            }))(reader);

    private static IParseResult<double> Exponential(TokenReader reader) =>
        (_Exponential ??=
            Sequence(
                Atom, 
                Parser.Handle(
                    SeparatedBy(Literal('^'), Exponential), 
                    fail => new Token<IList<double>>(fail.Offset, 0, Array.Empty<double>())))
                .As(vars =>
                {
                    var (b, pows) = vars;

                    if (pows.Count == 0)
                        return b;

                    var pow = pows[pows.Count - 1];

                    for (var i = pows.Count - 2; i >= 0; i--)
                        pow = Math.Pow(pows[i], pow);

                    return pow;
                }))(reader);


    private static Regex PlusMinus = PlusMinusRegex();

    private static Regex MulDiv = MulDivRegex();

    [GeneratedRegex("[+-]")]
    private static partial Regex PlusMinusRegex();

    [GeneratedRegex("[*/]")]
    private static partial Regex MulDivRegex();

}