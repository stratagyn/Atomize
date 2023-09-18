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
        .Then<ReadOnlyMemory<char>, double>(sign =>
                Choose(
                    Literal(Pattern.Text.Integer).As(n => double.Parse(n.Span)),
                    Island(Literal('('), Additive, Literal(')')))
                .As(n => sign.SemanticValue.Span[0] == '-' ? -n : n));


    private static Parser<double>? _Additive;
    private static Parser<double>? _Multiplicative;
    private static Parser<double>? _Exponential;

    private static IParseResult<double> Additive(TokenReader reader) =>
        (_Additive ??= 
            Fallback(Multiplicative, Literal(PlusMinus).As(c => (double)(int)c.Span[0]), Additive)
            .As(vars =>
            {
                var (x, y) = (vars[0], vars[2]);

                return vars[1] == '+' ? x + y : x - y;
            }))(reader);

    private static IParseResult<double> Multiplicative(TokenReader reader) =>
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