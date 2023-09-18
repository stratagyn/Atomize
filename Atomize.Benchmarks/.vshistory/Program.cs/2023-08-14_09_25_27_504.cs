using Atomize.Benchmarks;

var expression = "123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456";

var parse = BasicArithmeticCalculator.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.Value);
