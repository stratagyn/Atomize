﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize.Benchmarks;

[MemoryDiagnoser]
public class CalculatorBenchmarks
{
    [Params(
        "123+4567",
        "123+456*123-4567",
        "123+456*123-456/123+456*123-4567",
        "123+456*123-456/123+456*123-456/123+456*123-456/123+456*123-4567"]
    public string Expression;

    [Benchmark]
    public void BasicArithmetic() => BasicArithmeticCalculator.Parse(Expression);
}