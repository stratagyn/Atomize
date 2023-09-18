namespace Atomize;

public readonly struct EmptyToken<T> : IParseResult<T>
{
    public EmptyToken(int at) => Offset = at;

    public bool IsToken => true;

    public int Length => 0;

    public int Offset { get; }

    public string Conflict => "";

    public T? Value => default;
}

public readonly struct Lexeme<T> : IParseResult<T>
{
    public Lexeme(int at, int length, T value)
    {
        Length = length;
        Offset = at;
        Value = value;
    }

    public bool IsToken => true;

    public int Length { get; }

    public int Offset { get; }

    public string Conflict => "";

    public T Value { get; }
}