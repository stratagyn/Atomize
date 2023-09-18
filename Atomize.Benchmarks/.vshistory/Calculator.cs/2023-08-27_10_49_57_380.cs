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
    private static readonly Parser<double> Atom =
        Bind(
            Optional(Choice(Token('+'), Token('-'))),
            sign =>
                Map(
                    Choice(
                        Map(Text.Integer, n => double.Parse(n.Span)),
                        Island(Token('('), Additive, Token(')'))),
                    n => sign.Value == '-' ? -n : n));

    private static Parser<double>? _Additive;
    private static Parser<double>? _Multiplicative;
    private static Parser<double>? _Exponential;

    public static IParseResult<double> Parse(string expression) => Additive(new(expression, true));

    private static IParseResult<double> Additive(TextScanner scanner)
    {
        return (_Additive ??= Map(
            TryBind(
                Multiplicative,
                x => Bind(
                    Choice(Token('+'), Token('-')),
                    op => Map(Additive!,
                        y => op.Value == '-' ? x.Value - y : x.Value + y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1))(scanner);
    }

    private static IParseResult<double> Multiplicative(TextScanner scanner)
    {
        return (_Multiplicative ??= Map(
            TryBind(
                Exponential,
                x => Bind(
                    Choice(Token('*'), Token('/')),
                    op => Map(
                        Multiplicative!,
                        y => op.Value == '*' ? x.Value * y : x.Value / y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1))(scanner);
    }

    private static IParseResult<double> Exponential(TextScanner scanner)
    {
        return (_Exponential ??= Map(
        Split(Atom!, Token('^')),
        vars =>
        {
            var pow = vars[vars.Count - 1];

            for (var i = vars.Count - 2; i >= 0; i--)
                pow = Math.Pow(vars[i], pow);

            return pow;
        }))(scanner);
    }
}

public static class BasicArithmeticCalculator2
{
    private static readonly Parser<double> Additive;
    private static readonly Parser<double> Multiplicative;
    private static readonly Parser<double> Exponential;
    private static readonly Parser<double> Atom;

    static BasicArithmeticCalculator2()
    {
        Atom = Bind(
            Optional(Choice(Token('+'), Token('-'))),
            sign =>
                Map(
                    Choice(
                        Map(Parse.Text.Integer, n => double.Parse(n.Span)),
                        Island(Token('('), Additive!, Token(')'))),
                    n => sign.Value == '-' ? -n : n));

        Exponential = Map(
            Split(Atom!, Token('^')),
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
                    Choice(Token('*'), Token('/')),
                    op => Map(
                        Multiplicative!,
                        y => op.Value == '*' ? x.Value * y : x.Value / y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);

        Additive = Map(
            TryBind(
                Multiplicative,
                x => Bind(
                    Choice(Token('+'), Token('-')),
                    op => Map(Additive!,
                        y => op.Value == '-' ? x.Value - y : x.Value + y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);


    }

    public static IParseResult<double> Parse(string expression) => Additive(new(expression, true));
}