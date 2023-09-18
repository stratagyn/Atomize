using Atomize;
using Atomize.Benchmarks;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/* var expression = "(1  +  (2 ^ 2 ^ 3)  +  4)  -  ((5  *  6  /  7)  +  8  *  9)  /   - 10";
var parse = BasicArithmeticCalculator.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.Offset);
Console.WriteLine(parse.Length);
Console.WriteLine(parse.Value); */

//var _ = BenchmarkRunner.Run<CalculatorBenchmarks>();

var regexParser = Map(
    Parser.Token(Pattern.Text.RegularExpression),
    chars => new Regex(;
var test = "/\\/(.*(?<!\\\\))\\//";
var result = regexParser(new(test));

Console.WriteLine(result.IsToken);
Console.WriteLine(result.Offset);
Console.WriteLine(result.Length);