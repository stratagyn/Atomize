namespace Atomize;

public readonly struct EmptyToken<T> : IParseResult<T>
{
   public EmptyToken(int at) => Offset = at;

   public bool IsMatch => true;

   public int Length => 0;

   public int Offset { get; }

   public string Why => "";

   public T? Value => default;
}

public readonly struct Lexeme : IParseResult<ReadOnlyMemory<char>>
{
   public Lexeme(int at, int length, ReadOnlyMemory<char> text)
   {
      Length = length;
      Offset = at;
      Value = text;
   }

   public bool IsMatch => true;

   public int Length { get; }

   public int Offset { get; }

   public string Why => "";

   public ReadOnlyMemory<char> Value { get; }
}

public readonly struct Token<T> : IParseResult<T>
{
   public Token(int at, int length, T value)
   {
      Length = length;
      Offset = at;
      Value = value;
   }

   public bool IsMatch => true;

   public int Length { get; }

   public int Offset { get; }

   public string Why => "";

   public T Value { get; }
}