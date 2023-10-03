# `Parse`

Static methods for building and combining parsers.

## Recognizers

These methods either are themselves, or return *terminal* parsers, which match literal text.

Succeeds if `scanner` starts with `token`. Fails otherwise.

```cs
Parser<char> Atom(char parser);
Parser<ReadOnlyMemory<char>> Atom(string parser);
Parser<ReadOnlyMemory<char>> Atom(Regex parser);
```

Always succeeds returning an empty token.

```cs
IParseResult<T> Empty<T>(TextScanner scanner)
```

Succeeds when `scanner` is exhausted. Fails otherwise.

```cs
IParseResult<char> EndOfText(TextScanner scanner)
```

Always fails.

```cs
IParseResult<T> Fail<T>(TextScanner scanner)
```

Succeeds and ignores `token` if `scanner` starts with it. Fails otherwise.

```cs
Parser<char> Ignore(char parser);
Parser<string> Ignore(string parser);
Parser<string> Ignore(Regex parser);
Parser<T> Ignore<T>(Parser<T> parser);
```

Succeeds and without consuming `token` if `scanner` starts with it. Fails otherwise.

```cs
Parser<T> Peek<T>(Parser<T> parser);
```

Succeeds when `scanner` has not consumed any characters. Fails otherwise.

```cs
IParseResult<T> StartOfText<T>(TextScanner scanner)
```

Always succeeds returning any unconsumed characters between the current offset of the scanner and the index of `token`. If matching fails, the remaining unscanned characters are returned.

```cs
Parser<ReadOnlyMemory<char>> Until(char parser);
Parser<ReadOnlyMemory<char>> Until(string parser);
Parser<ReadOnlyMemory<char>> Until(Regex parser);
Parser<ReadOnlyMemory<char>> Until<T>(Parser<T> parser);
```

## State Management

These methods manage transitions from one parser to another, or from one result to another. They are useful for giving semantic meaning to parsed characters, recovering from or altering failure states, combining the results of several parsers, or conditional parsing.

Applies `bind` to the **successful** result of `parser` returning a new parser. Returns the failed result within the new parser, otherwise.

```cs
Parser<U> Bind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind)
```

Applies `condition` without consuming input, Returns the result of `then` if `condition` succeeds, `alt` otherwise.

```cs
Parser<U> If<T, U>(Parser<T> condition, Parser<U> then, Parser<U> alt)
```

Applies `map` to the value of the **successful** result of `parser`. Returns the failed result, otherwise.

```cs
Parser<T> Map<T, U>(Parser<T> parser, Func<T, U> map)
```

Applies `bind` to the **successful** result of `parser` returning a new parser, that returns a pair containing the first successful value, and parse result indicating whether or not the subsequent parser succeeded.

```cs
Parser<(T, IParseResult<U>)> Partial<T, U>(Parser<T> parser, Func<T, Parser<U>> bind)
```

## Recursion & Packrat

These methods create recursive and packrat parsers.

Returns the result of the parser returned after calling `lazyParser`.

```cs
Parser<T> Ref(Func<Parser<T>> lazyParser);
```

Returns a packrat parser with no support for left recursion.

```cs
Parser<T> Memoize(Parser<T> parser);
```

Returns a packrat parser with support for directly left recursive parsers.

```cs
Parser<T> DirectLeftRecursion(Parser<T> parser);
```

Returns a packrat parser with support for directly or indirectly left recursive parsers.

```cs
Parser<T> LeftRecursion(Parser<T> parser);
```

## Combinators

These methods combine and alter parsers into other parsers.

Returns the result of the first one to succeed in `parsers`. Fails if all parsers fail. If `parsers.Length` is `0`, it returns the always successful *empty* parser.

```cs
Parser<T> Choice<char>(params char[] parsers);
Parser<T> Choice<ReadOnlyMemory<char>>(params Regex[] parsers);
Parser<T> Choice<ReadOnlyMemory<char>>(params string[] parsers);
Parser<T> Choice<T>(params Parser<T>[] parsers);
```

Succeeds if `parser` succeeds `n` times. Fails if `n` is negative or less than `n` matches are made.
```cs
Parser<IList<T>> Exactly<T>(int n, Parser<T> parser)
```

Applies `assertion` without consuming characters, returning an empty token, if `assertion` succeeds. Fails otherwise.

```cs
Parser<T> FollowedBy<T> (Parser<T> assertion)
```

Sequentially applies `token`, then `next`, returning the result of `token`, if `next` succeeds, and consuming only the characters matched by `token`. Fails if either fails.
```cs
Parser<T> IfFollowedBy<T, F> (Parser<T> token, Parser<F> next)
```

Sequentially applies `token`, then `next`, returning the result of `token`, if `next` fails, consuming the characters matched by `token`. Fails if either `token` fails or `next` succeeds.

```cs
Parser<T> IfNotFollowedBy<T, F> (Parser<T> token, Parser<F> next)
```

Sequentially applies `token`, then searches backwards for a match to `previous`, returning the result of `token`, if `previous` match fails, consuming the characters matched by `token`. Fails if `token` fails or `previous` succeeds.

```cs
Parser<T> IfNotPrecededBy<T, F> (Parser<T> token, Parser<F> next)
```

Sequentially applies `token`, then searches backwards for a match to `previous`, returning the result of `token`, if `previous` match succeeds and ends at the current offset, and consuming only the characters matched by `token`. Fails if either fails.
```cs
Parser<T> IfPrecededBy<T, P>(Parser<T> token, Parser<P> previous)
```
Sequentially applies `open`, `token`, then `close`, returning only the result of `token` if all three succeed. Fails if any parser fails.
```cs
Parser<T> Island<O, T, C>(Parser<O> open, Parser<T> token, Parser<C> close)
```
Applies `separator` in between sequential applications of each parser in `parsers`. Stopping when either fails. Fails if any parser in `parsers` fails, or `separator` fails before applying all parsers.
```cs
Parser<IList<T>> Join<T, S>(Parser<S> separator, params Parser<T> parsers)
```
Tries to match `parser` at most `n` times. Fails if `n` is negative.
```cs
Parser<IList<T>> Maximum<T>(int n, Parser<T> parser)
```
Succeeds if `parser` succeeds at least `n` times. Fails if `n` is negative or less than `n` matches are made.
```cs
Parser<IList<T>> Minimum<T>(int n, Parser<T> parser)
```

Succeeds if `parser` succeeds either less than or greater `n` times. Fails if `n` is negative or exactly `n` matches are made.

```cs
Parser<IList<T>> NotExactly<T>(int n, Parser<T> parser)
```
Sequentially applies `token`, then `next`, returning the result of `token`, if `next` fails. Fails if `token` fails or `next` succeeds.
```cs
Parser<T> NotFollowedBy<T, F>(Parser<T> token, Parser<F> next)
```
Sequentially applies `token`, then searches backwards for a match to `previous`, returning the result of `token`, if `previous` fails to find a match that ends at the current offset. Fails if `token` fails or `previous` succeeds.
```cs
Parser<T> NotPrecededBy<T, P>(Parser<T> token, Parser<P> previous)
```
Always succeeds returning either the result of `parser` if it succeeds or an empty token.
```cs
Parser<T> Optional<T>(Parser<T> parser)
```
Tries to match both `first` and `second` in sequence if possible. If not, tries to match either of them. Fails if both fail.
```cs
Parser<(IParseResult<T>, IParseResult<U>)> Or<T, U>(Parser<T> first, Parser<U> second)
```

Searches backwards for a match to `previous`, returns empty token if `previous` match succeeds and ends at the current offset. Fails otherwise.

```cs
Parser<T> PrecededBy<T, P>(Parser<P> previous)
```

Succeeds if `parser` succeeds at least `min` times making at most `max` matches. Fails if `min` is negative, `max` is less than `min`, or less than `min` matches are made.
```cs
Parser<IList<T>> Repeat<T>(int min, int max, Parser<T> parser)
```

Returns a lazy reference to a parser.

```cs
Parser<T> Ref<T>(Func<Parser<T>> parserRef);
```

Applies a predicate to the `parser` if it succeeds. Fails if `parser` fails, or its result does not pass `test`.
```cs
Parser<T> Satisfies<T>(Parser<T> parser, Predicate<T> test)
```
Alternates applying `parser`, then `separator`. Stopping when either fails. Fails if `parser` fails to make at least one match.
```cs
Parser<IList<T>> SeparatedBy<T, S>(Parser<T> parser, Parser<S> separator)
```
Returns any text preceding successfully parsing `parser`. If `parser` fails, the rest of the unparsed text is returned.
```cs
Parser<ReadOnlyMemory<char>> Until<T>(Parser<T> parser)
```
Always succeeds returning the result of applying `parser` until it fails.
```cs
Parser<IList<T>> ZeroOrMore<T>(Parser<T> parser)
```
