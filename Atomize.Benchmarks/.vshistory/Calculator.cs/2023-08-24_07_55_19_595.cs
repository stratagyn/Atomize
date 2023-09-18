using System.Text.RegularExpressions;

using static Atomize.Parser;

namespace Atomize.Benchmarks;

/*
 *  Additive: Multiplicative  /[+-]/  Additive | Multiplicative;
 *  Multiplicative: Exponential  /[*\/]/  Multiplicative | Exponential;
 *  Exponential: Atom ('^' Exponential) * ;
 *  Atom:  /[+-]?/  NUMBER | '(' Additive ')'
 */

public static partial class BasicArithmeticCalculator
{
    private static readonly Parser<double> Atom =
        Bind(
            Optional(Choose(Token('+'), Token('-'))),
            sign =>
                Map(
                    Choose(
                        Map(Token(Pattern.Text.Integer), n => double.Parse(n.Span)),
                        Island(Token('('), Additive, Token(')'))),
                    n => sign.Value == '-' ? -n : n));

    private static readonly Parser<double> Additive = Map(
            TryBind(
                Multiplicative,
                x => Bind(
                    Choose(Token('+'), Token('-')),
                    op => Map(Additive,
                        y => op.Value == '-' ? x.Value - y : x.Value + y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);

    private static readonly Parser<double> Multiplicative = Map(
            TryBind(
                Exponential,
                x => Bind(
                    Choose(Token('*'), Token('/')),
                    op => Map(
                        Multiplicative,
                        y => op.Value == '*' ? x.Value * y : x.Value / y))),
            vars => vars.Item2.IsToken ? vars.Item2.Value : vars.Item1);

    private static Parser<double>? _Exponential;

    public static IParseResult<double> Parse(string expression) => Additive(new(expression, true));

    private static IParseResult<double> Exponential(TextScanner reader) =>
        (_Exponential ??= Map(Split(Atom, Token('^')),
            vars =>
            {
                var pow = vars[vars.Count - 1];

                for (var i = vars.Count - 2; i >= 0; i--)
                    pow = Math.Pow(vars[i], pow);

                return pow;
            }))(reader);
}