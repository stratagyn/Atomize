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
        )]
    public string Expression; 
}