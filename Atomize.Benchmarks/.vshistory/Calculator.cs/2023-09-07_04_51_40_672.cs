﻿using System.Text.RegularExpressions;

using static Atomize.Parse;

namespace Atomize.Benchmarks;

/*
 *  Additive: Multiplicative  /[+-]/  Additive | Multiplicative;
 *  Multiplicative: Exponential  /[*\/]/  Multiplicative | Exponential;
 *  Exponential: Atom ('^' Exponential) * ;
 *  Atom:  /[+-]?/  NUMBER | '(' Additive ')'
 */

public static class BasicArithmeticCalculator
{
    private static readonly Parser<double> Additive;
    private static readonly Parser<double> Multiplicative;
    private static readonly Parser<double> Exponential;
    private static readonly Parser<double> Atom;

    static BasicArithmeticCalculator()
    {
        Atom = Bind(
            Optional(Choice(Atomize.Parse.Atom('+'), Atomize.Parse.Atom('-'))),
            sign =>
                Map(
                    Choice(
                        Map(Text.Integer, n => double.Parse(n.Span)),
                        Island(Atomize.Parse.Atom('('), Additive!, Atomize.Parse.Atom(')'))),
                    n => sign == '-' ? -n : n));

        Exponential = Map(
            SeparatedBy(Atom!, Atomize.Parse.Atom('^')),
            vars =>
            {
                var pow = vars[vars.Count - 1];

                for (var i = vars.Count - 2; i >= 0; i--)
                    pow = Math.Pow(vars[i], pow);

                return pow;
            });

        Multiplicative = Map(
            TryBind(
                Exponential,
                x => Bind(
                    Choice(Atomize.Parse.Atom('*'), Atomize.Parse.Atom('/')),
                    op => Map(
                        Multiplicative!,
                        y => op == '*' ? x * y : x / y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);

        Additive = Map(
            TryBind(
                Multiplicative,
                x => Bind(
                    Choice(Atomize.Parse.Atom('+'), Atomize.Parse.Atom('-')),
                    op => Map(Additive!,
                        y => op == '-' ? x - y : x + y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);
    }

    public static IParseResult<double> Parse(string expression) => Additive(new(expression, true));
}