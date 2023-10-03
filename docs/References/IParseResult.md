# `IParseResult<T>`

The interface for results returned from parse attempts.

## Properties

Whether the result is success or failure

```cs
bool IsToken { get; }
```

Number of characters matched

```cs
int Length { get; }
```

Where in the text the match was attempted

```cs
int Offset { get; }
```

Reason why a successful match was not made

```cs
string Error { get; }
```

The token of a successful parse

```cs
T? Value { get; }
```

## Implementations

### `EmptyToken<T>`

A successful parse that records no information abbout the characters consumed, i.e. the `Length` of this token is always `0`, even if it's from a parser that consumed characters, e.g. parsers returned by `Ignore`.

```cs
public EmptyToken(int at)
```

### `Lexeme<T>`

A successful parse at the given `Offset`. `Length` characters were consumed. `Value` represents these characters or some semantic meaning of them.

```cs
public Lexeme(int at, int length, T value)
```

### `Failure<T>`

A failed parse at the given `Offset`, with `Why` explaining the reason, all other properties are `default`.

```cs
public Failure(int at, string reason)
```

