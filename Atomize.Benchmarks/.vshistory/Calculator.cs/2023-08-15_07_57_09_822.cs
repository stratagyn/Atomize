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
public static partial class BasicArithmeticCalculator
{
    private static Parser<double> Atom =
        Bind(
            Optional(Token(PlusMinusRegex())),
            sign =>
                Map(
                    Choose(
                        Map(Token(Pattern.Text.Integer), n => double.Parse(n.Span)),
                        Island(Token('('), Additive, Token(')'))),
                    n => sign.Value.IsChar('-') ? -n : n));


    private static Parser<double>? _Additive;
    private static Parser<double>? _Multiplicative;
    private static Parser<double>? _Exponential;

    public static IParseResult<double> Parse(string expression) => Additive(new(expression));

    private static IParseResult<double> Additive(Scanner reader) =>
        (_Additive ??= Map(
            TryBind(
                Multiplicative, 
                x => Bind(
                    Token(PlusMinus),
                    op => Map(
                        Additive, 
                        y => op.Value.IsChar('-') ? x.Value - y : x.Value + y))),
            vars => vars.Item2.IsToken ? vars.Item2 : vars.Item1))(reader);

    private static IParseResult<double> Multiplicative(Scanner reader) =>
        (_Multiplicative ??= Map(
            TryBind(
                Exponential,
                x => Bind(
                    Token(MulDiv),
                    op => Map(
                        Multiplicative,
                        y => op.Value.IsChar('*') ? x.Value * y : x.Value / y))),
            vars => vars.Item2.IsToken ? vars.Item2 : vars.Item1))(reader);

    private static IParseResult<double> Exponential(Scanner reader) =>
        (_Exponential ??= Split(Token('^'), Atom).Map(
            vars =>
            {
                var pow = vars[vars.Count - 1];

                for (var i = vars.Count - 2; i >= 0; i--)
                    pow = Math.Pow(vars[i], pow);

                return pow;
            }))(reader);


    private static Regex PlusMinus = PlusMinusRegex();


    private static Regex MulDiv = MulDivRegex();


    [GeneratedRegex("[+-]")]
    private static partial Regex PlusMinusRegex();


    [GeneratedRegex("[*/]")]
    private static partial Regex MulDivRegex();
}
