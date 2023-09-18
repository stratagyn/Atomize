namespace Atomize;

public interface IParseResult<T>
{
    public bool IsToken { get; }
    public int Length { get; }
    public int Offset { get; }
    public string Error { get; }
    public T? Value { get; }
}