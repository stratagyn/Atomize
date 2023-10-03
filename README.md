# `Atomize/__Quickstart__`

![](https://img.shields.io/badge/passing-true-green)
![](https://img.shields.io/badge/coverage-97.2%25-green)

`Atomize` is a combinatory parser building library supporting the standard PEG operators, as well as: 

* Positive and negative look-behind
* Directly and indirectly left recursive parsers

## Installation

`Atomize` can be installed via NuGet or

```plaintext
dotnet add package Atomize
```

## Documentation

[User Guide](https://github.com/stratagyn/Atomize/blob/master/docs/UserGuide.md)  
[References](https://github.com/stratagyn/Atomize/tree/master/docs/References)

## Example: `BasicCalculator`

Implementing a parser based on the following grammar:

```peg
Additive
: Additive /[+-]/ Multiplicative
| Multiplicative;

Multiplicative
: Multiplicative /[*\/]/ Atomic
| Atomic;

Atomic
: NUMBER
| '(' Additive ')';
```

```cs
using static Atomize.Parse;

public static class BasicCalculator
{
   private static readonly Parser<double> Atomic = 
      Choice(
         from n in Text.Number 
         select double.Parse(n.Span),

         Island(Atom('('), Ref(() => Additive!), Atom(')'))
      );

   private static readonly Parser<double> Multiplicative = 
      DirectLeftRecursion(
         Choice(
            from left in Ref(() => Multiplicative!)
            from op in Choice(Atom('*'), Atom('/')),
            from right in Atomic
            select op == '*'
               ? left * double.Parse(right.Span)
               : left / double.Parse(right.Span),
            
            Atomic
         )
      );    
   
   private static readonly Parser<double> Additive =
      DirectLeftRecursion(
         Choice(
            from left in Ref(() => Additive!)
            from op in Choice(Atom('+'), Atom('-')),
            from right in Multiplicative
            select op == '+'
               ? left + double.Parse(right.Span)
               : left - double.Parse(right.Span),
            
            Multiplicative)); 

   public static IParseResult<double> Apply(TextScanner expr) => 
      Additive(expr);
}
```
