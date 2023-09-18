using Atomize;
using Atomize.Benchmarks;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/*var expression = "(1  +  (2 ^ 2 ^ 3)  +  4)  -  ((5  *  6  /  7)  +  8  *  9)  /   - 10";
var parse = BasicArithmeticCalculator2.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.Offset);
Console.WriteLine(parse.Length);
Console.WriteLine(parse.Value);*/

var peggy = """
Grammar: (Rule ';')+ $;
Rule: IDENTIFIER ':' Choice;
Choice: Sequence ('|' Sequence)*;
Sequence: Greedy (' ' Greedy)*;
Greedy: Predicate ('*' | '+' | '?')?;
Predicate: ('&' | '!')? Atom;
Atom: IDENTIFIER | REGEX | LITERAL | '(' Choice ')';

IDENTIFIER: /[a-zA-Z_][a-zA-Z0-9_]*/;
REGEX: /\/(.*(?<!\\))\//;
LITERAL: /'(.*(?<!\\))'/;
""".Trim();

var result = PEGTokenizer.Parse(peggy);

Console.WriteLine(result.IsToken);
Console.WriteLine(result.Offset);
Console.WriteLine(result.Length);

if (!result.IsToken)
{
    var start = Math.Max(0, result.Offset - 5);
    var end = Math.Min(peggy.Length, result.Offset + 5);

    var before = peggy[start..result.Offset];
    var beforeU = string.Join("", Enumerable.Range(0, before.Length).Select(_ => ' '));
    var after = peggy[(result.Offset + 1)..end];

    Console.WriteLine($"\n{before}{peggy[result.Offset]}{after}");
    Console.WriteLine($"{beforeU}{'^'}\n");
    Console.WriteLine(result.Conflict);
}

//var _ = BenchmarkRunner.Run<CalculatorBenchmarks>();

/*var regexParser = Parser.Map(
    Parser.Token(Pattern.Text.RegularExpression),
    chars => new Regex(new string(chars.Span)));

var test = "/\\/(.*(?<!\\\\))\\//";

var result = regexParser(new(test));

Console.WriteLine(result.IsToken);
Console.WriteLine(result.Offset);
Console.WriteLine(result.Length);*/