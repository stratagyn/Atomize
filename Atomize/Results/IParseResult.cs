namespace Atomize;

public interface IParseResult<T>
{
   public bool IsMatch { get; }
   public int Length { get; }
   public int Offset { get; }
   public T? Value { get; }
   public string Why { get; }
}