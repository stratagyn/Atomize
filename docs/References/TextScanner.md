# `TextScanner`

Wraps raw text, enabling character-by-character advancement and backtracking through the provide `string`.

## Initialization

Constructs a new scanner over `text`. If `squeeze` is `true`, scanning skips over 
non-quoted whitespace. Quoted text in this case is any single- or double-quoted 
substring.

```cs
TextScanner(string text, bool squeeze = false)
```

## Properties

The current location of the scanner in the raw text. This differs from the offset, which considers whether or not whitespace is being ignored.

```cs
int Index { get; }
```

The total number of characters that have been scanned. This does not include whitespace if it is being ignored.

```cs
int Offset { get; }
```

The raw text being scanned.

```cs
string Text { get; }
```

## Methods

Moves the scanner `count` characters forward ignoring all consumed characters.

```cs
void Advance(int count = 1)
```

Moves the scanner `count` characters backwards

```cs
void Backtrack(int count = 1)
```

Moves the scanner `count` characters forward returning all consumed characters.

```cs
ReadOnlyMemory<char> Read(int count = 1)
```

Moves the scanner to the end returning all consumed characters.

```cs
ReadOnlyMemory<char> ReadToEnd()
```

Moves the scanner back to the beginning

```cs
void Reset()
```

