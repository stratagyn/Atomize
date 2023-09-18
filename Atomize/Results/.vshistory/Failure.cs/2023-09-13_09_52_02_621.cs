namespace Atomize;

public readonly struct Failure<T> : IParseResult<T>
{
    internal Failure(int offset, string reason)
    {
        Offset = offset;
        Error = reason;
    }

    public bool IsToken => false;

    public int Length => 0;

    public int Offset { get; }

    public string Error { get; }

    public T? Value => default;

    public override string ToString() =>
        $"Failure @[{Offset}] :: {Error}";
}