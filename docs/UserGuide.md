[IParseResult]: ./References/IParseResult.md
[Parse]: ./References/Parse.md
[Patterns]: ./References/Patterns.md
[TextScanner]: ./References/TextScanner.md

[ReadOnlyMemory]: https://learn.microsoft.com/en-us/dotnet/api/system.readonlymemory-1?view=net-7.0

# `Atomize/__UserGuide__`

`Atomize` is a combinatory parser building library. 

## Table of Content
[Scanning Text](#scanning-text)  
[Parsing Text](#parsing-text)  
[Building Parsers](#building-parsers)  
&emsp; [Recognizers](#recognizers)  
&emsp; [Recursion & Packrat Parsers](#recursion--packrat-parsers)  
&emsp; [Conditional Parsers](#conditional-parsers)   
&emsp; [Repetition Parsers](#repetition-parsers)  
&emsp; [Helper Parsers](#helper-parsers)
 

## Scanning Text

[`TextScanner`][TextScanner] wraps raw text enabling character-by-character scanning with arbitrary advancement and backtracking. Text can optionally be "*squeezed*" of any non-quoted whitespace.

```cs
var scanner1 = new TextScanner("abcdefghijklmnopqrstuvwxyz");

var scanner2 = new TextScanner(
    "a b c d e f g h i j k l m n o p q r s t u v w x y z", 
    true);

Console.WriteLine($"`scanner1`\n----------\n);
Console.WriteLine($"Offset is: {scanner1.Offset}");
Console.WriteLine($"Index is: {scanner1.Index}\n");

Console.WriteLine($"Read lowercase letters: {new string(scanner1.Read(26).Span)}");
Console.WriteLine($"Offset after read is: {scanner1.Offset}");
Console.WriteLine($"Index after read is: {scanner1.Offset}\n");

scanner1.Backtrack(15);

Console.WriteLine($"Offset after backtrack is: {scanner1.Offset}");
Console.WriteLine($"Index after backtrack is: {scanner1.Index}\n");

scanner.Advance(8);

Console.WriteLine($"Offset after advance is: {scanner1.Offset}");
Console.WriteLine($"Index after advance is: {scanner1.Index}\n");

Console.WriteLine($"Read To End: {new string(scanner2.ReadToEnd().Span)}\n");

Console.WriteLine($"`scanner2`\n--------\n);
Console.WriteLine($"Offset is: {scanner2.Offset}");
Console.WriteLine($"Index is: {scanner2.Index}\n");

Console.WriteLine($"Read lowercase letters: {new string(scanner2.Read(26).Span)}");
Console.WriteLine($"Offset after read is: {scanner2.Offset}");
Console.WriteLine($"Index after read is: {scanner2.Offset}\n");

scanner1.Backtrack(15);

Console.WriteLine($"Offset after backtrack is: {scanner2.Offset}");
Console.WriteLine($"Index after backtrack is: {scanner2.Index}\n");

scanner.Advance(8);

Console.WriteLine($"Offset after advance is: {scanner2.Offset}");
Console.WriteLine($"Index after advance is: {scanner2.Index}\n");

Console.WriteLine($"Read To End: {new string(scanner2.ReadToEnd().Span)}");

/*
`scanner1`
----------

offset is: 0
index is: 0

Read: abcdefghijklmnopqrstuvwxyz
Offset after read is: 26
Index after read is: 26

Offset after backtrack is: 11
Index after backtrack is: 11

Offset after advance is: 19
Index after advance is: 19

Read To End: tuvwxyz

`scanner2`
----------

offset is: 0
index is: 0

Read: abcdefghijklmnopqrstuvwxyz
Offset after read is: 26
Index after read is: 51

Offset after backtrack is: 11
Index after backtrack is: 21

Offset after advance is: 19
Index after advance is: 37

Read To End: tuvwxyz
 */

```

### See
* [`TextScanner`][TextScanner]

### External
* [`ReadOnlyMemory<char>`][ReadOnlyMemory]

## Parsing Text

Parsers are functions that anaylze text producing some arbitrary structure. `Atomize` provides methods for building recursive-descent parsers, using combinators to create more complex parsers from simpler ones.

All parsers return an `IParseResult<T>` object indicating whether it successfully matched a prefix. In the case of success, the result is either an `EmptyToken<T>` &mdash; representing unconsumed or consumed but ignored text &mdash; or a `Lexeme<T>` &mdash; corresponding to parsed text that can be given further meaning. In the case of failure, a `Failure<T>` object is returned with the explanation of the error.

```cs
using static Atomize.Parse;

var scanner = new TextScanner("abcdefghijklmnopqrstuvwxyz0123456789");

var digitParser = Text.Digit;
var lowercaseParser = Text.LowercaseLetter;

Console.WriteLine($"Scanner is looking at char: {scanner.Offset}\n");

var parseResult = digitParser(scanner);

Console.WriteLine($"Parsed digits at {parseResult.Offset}? {parseResult.IsToken});
Console.WriteLine($"Reason for failure: {parseResult.Why}\n");

Console.WriteLine($"Scanner is looking at char: {scanner.Offset}\n");

parseResult = lowercaseParser(scanner);

Console.WriteLine($"Parsed lowercase letters at {parseResult.Offset}? {parseResult.IsToken});
Console.WriteLine($"Number of characters matched: {parseResult.Length}\n");

Console.WriteLine($"Scanner is looking at char: {scanner.Offset}\n");

parseResult = digitParser(scanner);

Console.WriteLine($"Parsed digits at {parseResult.Offset}? {parseResult.IsToken});
Console.WriteLine($"Number of characters matched: {parseResult.Length}\n");

Console.WriteLine($"Scanner is looking at char: {scanner.Offset}\n");

var eot = EndOfText(scanner);

Console.WriteLine($"Parsed end of text? {eot.IsToken})

/*
Scanner is looking at char: 0

Parsed digits at 0? false;
Reason for failure: Expected /[a-z]+/.

Scanner is looking at char: 0

Parsed lowercase letters at 0? true
Number of characters matched: 26

Scanner is looking at char: 26

Parsed digits at 26? true
Number of characters matched: 10

Scanner is looking at char: 36

Parsed end of text? true
 */
```

### See
* [`IParseResult`][IParseResult]
* [`Parse`][Parse]

## Building Parsers

The [`Parse`][Parse] class provides several static methods for creating and combining parsers.

#### `Bind`, `Map`, `Satisfies`

`Parser<T>` also supports sequencing and transformation using LINQ operators `SelectMany`, `Select`, and `Where`, mapping them to the `Bind`, `Map`, and `Satisfies` functions.

`Bind` takes a parser from one state to another.

`Map` takes the result of a parser from one type to another. 

`Satisfies` does a post-check of the result value against a predicate.

```cs
Parser<double> EnclosedNegativeNumber = 
   Satisfies(
      Bind(
         Choice('+', '-')
         sign => Map(
            Text.Number,
            val => sign == '-'
               ? -double.Parse(val.Span)
               : double.Parse(val.Span)
         ),
         n => n < 0
      )
   );
```

or using LINQ

```cs
Parser<double> EnclosedNegativeNumber = 
   from n in (
      from sign in Choice('+', '-')
      from val in Multiple.Number
      select sign == '-'
         ? -double.Parse(val.Span)
         : double.Parse(val.Span)
   )
   where n < 0
   select n;
```

#### `Partial`

`Partial` constructs partial match parsers. Used for parsing sequences with valid subsequences.

```cs
Parser<ILinkedList<int>> SequenceOfInts =
   Island(
      Ignore('['),

      from list in Partial(
         from n in Text.Digit 
         select int.Parse(n.Span),

         head => 
            from _ in Atom(",")
            from ints in Ref(() => SequenceOfInts!)
            select ints.AddFirst(head))
      select list.Full.IsMatch 
         ? list.Full.Value 
         : new LinkedList<int>(new int[] { list.Partial }),
         
      Ignore(']')
   );
```

This is a recursive definition of a list that is built by sequentially attaching elements to the front.

```cs
var result = SequenceOfInts(new TextScanner("[1,2,3,4,5,6]"));

Console.WriteLine($"[{string.Join(',', result.Value)}]"); //[1,2,3,4,5,6]
```

#### `Choice`

`Choice` implements _**ordered**_ choice. Given a list of parsers, it returns the result of the first one to succeed. This differs from the usual choice operator which returns the result of each option.

```cs
Parser<int> HexOrNatural = 
   Choice(
      from hex in Text.HexDigit 
      select Convert.ToInt32(hex.Span, 16),
      
      from dec in Text.Digit
      select int.Parse(dec.Span)
   );
```

### Recognizers

`Atom` matches literal text based on a `char`, `string`, or `Regex`.

`Empty` matches the empty string and is always successful.

`EndOfText` matches the end of text.

`Fail` is a parser that fails no matter what.

`Ignore` matches literal text based on a `char`, `string`, or `Regex`, consuming it blindly.

`Peek` matches literal text based on a `char`, `string`, or `Regex` without consuming it.

`StartOfText` matches the start of text.

`Parse.Single` and `Parse.Multiple` implement common regular expression parsers. They are described [here](./References/Patterns).

```cs
Parser<double> signedNumber = 
   from sign in Optional(Atom('-'))
   from num in Multiple.Number
   select sign == '-' 
      ? -double.Parse(num.Span) 
      : double.Parse(num.Span);
```

### Recursion & Packrat Parsers

#### `Ref`

`Ref` applies a lazily defined parser.

#### `Memoize`,

`Memoize` creates a simple packrat parser with **no** support for left recursion.

```peg
Additive
: Multiplicative /[+-]/ Additive
| Multiplicative;

Multiplicative
: Atomic /[*\/]/ Multiplicative
| Atomic;

Atomic
: NUMBER
| '(' Additive ')';
```

Rules like `Additive` and `Multiplicative` can lead to exponential parse times without use of packrat parsing. This is because these rules try to match a longer sequence, before matching an alternative that is a subsequence of it. This means the alternative is something we've parsed before with the first alternate, so it's doing redundant/repetitive work.

Applying a parser for the above grammar as-is to an expression like "(1 + 2)" would cause the following redundant parses:

|   Token   |Passes|            Reason          |
|:---------:|:----:|:--------------------------:|
| `(1 + 2)` |   4  | not followed by an operator
|    `1`    |   8  | not followed by `/[*\/]/`
|    `2`    |   16 | not followed by an operator

Passes increase exponentially with the number of parentheses.

Implementing a parser for the above grammar would also require some processing to make sure that `Additive` and `Multiplicative` are properly left-associative. However, with support for direct left recursion, we can instead implement them directly as:

#### `DirectLeftRecursion`

`DirectLeftRecursion` can handle directly left recursive parsers.

Consider the following grammar:

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

A parser for this grammar looks like this:

```cs
using static Atomize.Parse;

public static class BasicCalculator
{
    private static readonly Parser<double> Atomic = 
        Memoize(
            from sign in Optional(Choice('+', '-'))
            from n in Choice(
                from value in Text.Digit
                select double.Parse(value.Span),
                
                Island(Atom('('), Additive!, Atom(')')))
            select sign == '-' ? -n : n
        );

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
                
                Exponential
            )
        );

    private static readonly Parser<double> Additive = 
        DirectLeftRecursion(
            Choice(
                from x in Ref(() => Additive!)
                from op in Choice('+', '-')
                from y in Multiplicative
                select op == '-' ? x - y : x + y,

                Multiplicative
            )
        );

    public static IParseResult<double> Apply(TextScanner expression) => Additive(expression);
}
```

Applying this parser to `"1+2-3/4*5"` produces `-0.75`.

Support for indirect left recursion also allows for accomodating

#### `LeftRecursion`

`LeftRecursion` can handle either directly or indirectly left recursive parsers.

For example if the above grammar was given as:

```peg
Atomic:  /[+-]?/  (NUMBER | '(' Expression ')')
Exponential: Expression ('^' Expression)*
Multiplicative: Expression  (/[*\/]/ Expression)*
Additive: Expression (/[+-]/ Expression)*
Expression: Exponential | Multiplicative | Additive | Atomic
```

Though, like the first grammar, left-associativity is not inherent to the `Additive` and `Multiplicative`.

 It may be confusing understanding which parsers should be defined with `LeftRecursion`. The best way of going about this is to:

1. Find the *head* rule (`Expression` in the above example)
2. Implement **all** indirectly left recursive choices in this rule (`Exponential`, `Multiplicative`, `Additive`) with `LeftRecursion`.

Implementing `Expression` with `LeftRecursion` would cause an early and false success. For example parsing `"1+2"` would only parse `1`, since all alternatives will fail first and `Atomic` will ultimately succeed.

Left recursion algorithms are based on those in this [paper](https://web.cs.ucla.edu/~todd/research/pepm08.pdf).

### Conditional Parsers

#### Assertions

`FollowedBy` creates a positive lookahead parser.

`IfFollowedBy` takes a parser to apply before the assertion, succeeding only if both succeed.

`NotFollowedBy` creates a negative lookahead parser.

`IfNotFollowedBy` takes a parser to apply before the assertion, succeeding if the parser does and the assertion fails.

`PrecededBy` creates a positive look-behind parser.

`IfPrecededBy` takes a parser to apply before the assertion, succeeding only if both succeed.

`NotPrecededBy` creates a negative look-behind parser.

`IfNotPrecededBy` takes a parser to apply before the assertion, succeeding if the parser does and the assertion fails.

#### `If`

`If` constructs a conditional parser equivalent to:

```cs
Parser<T> If<C, T>(Parser<C> condition, Parser<T> then, Parser<T> alt) =>
   Choice(
      from _ in FollowedBy(condition)
      from parsed in then
      select parsed,
      
      alt
   );

Parser<int> HexOrNatural = 
   If(
      Atom("0x"),

      from hex in Text.HexDigit 
      select Convert.ToInt32(hex.Span, 16),
      
      from dec in Text.Digit
      select int.Parse(dec.Span)
   );
```

`HexOrNatural` parses strings that are either a sequence of numerical digits or hexadecimal digits prefixed with `"0x"`.

### Repetition Parsers

`Exactly` given a positive integer `n`, matches a given parser an exactly `n` times.

`Maximum` given a positive integer `n`, matches a given parser at least `0` and at most `n` times.

`Minimum` given a positive integer `n`, matches a given parser at least `n` times.

`NotExactly` given a positive integer `n`, matches a given parser less than or greater than `n` times.

`Optional` creates a parser that succeeds whether it matches a given parser or not.

`Repeat` given a positive integers `min` and `max`, matches a given parser at least `min` and at most `max` times.

`ZeroOrMore` matches a given parser as many times as possible

### Helper Parsers

#### `Island`

`Island` matches a parser surrounded on both sides, only returning what is consumed by the parser itself.

```cs
Parser<T> SurroundedByWhitespace<T>(Parser<T> parser) =>
   Island(
      Optional(Multiple.Whitespace),
      parser,
      Optional(Multiple.Whitespace)
   );
```

#### `Join`

`Join` matches multiple parsers with a common separator, ignoring the separator itself.


```cs
Parser<IList<ReadOnlyMemory<char>>> IdStringNatural = 
   Join(
      Single.NewLine,
      Multiple.Identifier,
      Mutliple.QuotedString,
      Multiple.Digit
   );
```

#### `SeparatedBy`

`SeparatedBy` matches a parser one or more times with a common separator, ignoring the separator itself.

```cs
Parser<IList<ReadOnlyMemory<char>>> Exponentiation =
   SeparatedBy(Text.Number, Atom('^'));
```

### See
* [`Parse`][Parse]
* [`Patterns`][Patterns]
