using Atomize.Benchmarks;

var expression = "(1+(2^2^3)+4)-((5*6/7)+8*9)/10";
var parse = IntegerCalculator.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.SemanticValue);
