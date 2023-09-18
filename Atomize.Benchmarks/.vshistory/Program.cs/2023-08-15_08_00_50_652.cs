﻿using Atomize.Benchmarks;
using BenchmarkDotNet.Running;

var expression = "(1+(2^2^3)+4)-((5*6/7)+8*9)/10";
var parse = BasicArithmeticCalculator.Parse(expression);

Console.WriteLine(parse.IsToken);
Console.WriteLine(parse.SemanticValue);

//var _ = BenchmarkRunner.Run<CalculatorBenchmarks>();