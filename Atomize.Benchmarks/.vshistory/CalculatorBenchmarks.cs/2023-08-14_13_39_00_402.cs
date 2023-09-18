using BenchmarkDotNet.Attributes;
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
        "((((((((((1+2+3+4+5+6+7+8+9+10)))))))))))",
        "123+456*123-456/123+456*123-456/123+456*123-456/123+456*123-4567",
        "(1+(2^2^3)+4)-((5*6/7)+8*9)/10")]
    public string Expression;

    [Benchmark]
    public void BasicArithmetic() => BasicArithmeticCalculator.Parse(Expression);
}
