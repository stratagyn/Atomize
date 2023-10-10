using System.Linq.Expressions;
using static Atomize.Parse;

namespace Atomize.Examples;

/*
 *  Additive: Additive  /[+-]/  Multiplicative | Multiplicative;
 *  Multiplicative: Multiplicative  /[*\/]/  Exponential | Exponential;
 *  Exponential: Atom ('^' Exponential)* ;
 *  Atomic:  /[+-]?/  (NUMBER | '(' Additive ')')
 */
public static class LeftRecursiveCalculator
{
   private static readonly Parser<double> Atomic = 
      Memoize(
         from sign in Optional(Choice('+', '-'))
         from n in Choice(
            from value in Multiple.Digit
            select double.Parse(value.Span),
            
            Island(Atom('('), Additive!, Atom(')')))
         select sign == '-' ? -n : n);

   private static readonly Parser<double> Exponential =
      Memoize(
         from pows in SeparatedBy(Atomic, Atom('^'))
         select pows.Reverse().Aggregate((p, b) => Math.Pow(b, p))
      );

   private static readonly Parser<double> Multiplicative =
      DirectLeftRecursion(
         Choice(
            from x in Ref(() => Multiplicative!)
            from op in Choice('*', '/')
            from y in Exponential
            select op == '*' ? x * y : x / y,
            
            Exponential)
      );

   private static readonly Parser<double> Additive = 
      DirectLeftRecursion(
         Choice(
            from x in Ref(() => Additive!)
            from op in Choice('+', '-')
            from y in Multiplicative
            select op == '-' ? x - y : x + y,

           Multiplicative)
      );

   public static IParseResult<double> Parse(string expression) => Additive(new(expression, true));
   public static IParseResult<double> Parse(TextScanner expression) => Additive(expression);

}


