namespace Atomize;


public readonly struct Parser<T>
{
    private readonly Func<TokenReader, T> _parser;

    internal Parser(Func<TokenReader, T> parser) =>
        _parser = parser;
}