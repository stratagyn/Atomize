using BenchmarkDotNet.Attributes;

namespace Atomize.Benchmarks;

[MemoryDiagnoser]
public class CalculatorBenchmarks
{
    [Params(
        "12 + 3 - 4 * 56 / 7",
        "123 + 456 * 123 - 45 / 67",
        "123 + 456 * 123 - 456 / 123 + 456 * 123 - 4567",
        "123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 456 / 123 + 456 * 123 - 4567")]
    public string Expression;

    [Benchmark]
    public void BasicArithmetic() => BasicArithmeticCalculator.Parse(Expression);
}