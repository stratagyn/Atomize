using Atomize;
using Atomize.Benchmarks;
using BenchmarkDotNet.Running;

/* var expression = "(1  +  (2 ^ 2 ^ 3)  +  4)  -  ((5  *  6  /  7)  +  8  *  9)  /   - 10";
var parse = BasicArithmeticCalculator.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.Offset);
Console.WriteLine(parse.Length);
Console.WriteLine(parse.Value); */

//var _ = BenchmarkRunner.Run<CalculatorBenchmarks>();

var regexParser = Parser.Token(Pattern.Text.RegularExpression);
var test = "/\\/(.*(?<!\\\\))\\//"