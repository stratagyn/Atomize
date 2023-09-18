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
                    n => sign.Length > 0 && sign.Value.Span[0] == '-' ? -n : n));


    private static Parser<double>? _Additive;
    private static Parser<double>? _Multiplicative;
    private static Parser<double>? _Exponential;

    public static IParseResult<double> Parse(string expression) => Additive(new(expression));

    private static IParseResult<double> Additive(Scanner reader) =>
        (_Additive ??= Parser.ThenTry(
            Multiplicative, 
            x => Token(PlusMinus).Then<ReadOnlyMemory<char>, double>(
                op => Parser.As(Additive, 
                    y =>
                    {

                    })))(reader);

    private static IParseResult<double> Multiplicative(Scanner reader) =>
        (_Multiplicative ??= Fallback(Exponential, Token(MulDiv), Multiplicative).As(
            vars =>
            {
                if (vars.Item2.Length == 0)
                    return vars.Item1;

                var (x, y) = (vars.Item1, vars.Item3);

                return vars.Item2.Span[0] == '*' ? x * y : x / y;
            }))(reader);

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
