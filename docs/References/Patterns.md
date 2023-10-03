# Common Regular Expressions

Some common regular expressions are available as parsers. They are split into two sub-classes `Parse.Single` for single character expressions, `Parse.Multiple` for matching *one-or-more* characters. The table below list the currently available expressions.

## Table of Expressions

| Pattern             | `Character`                | `Text` 
|:------------------- |:--------------------- |:----------
| Alphanumeric        | `[a-zA-Z0-9]`         |  `[a-zA-Z0-9]+`
| Any                 | `.`                   |  `.+`
| Control             | `[\x00-\x1F\x7F]+`    |  `[\x00-\x1F\x7F]+`
| Digit               | `[0-9]`               | `[0-9]+`
| DoubleQuotedString  |                       | `"(.*(?<!\\))"`
| EscapeSequence      | `\\[\\'0abfnrtv""]`   | `(\\[\\'0abfnrtv""])+`
| EscapedHex          | `\\x[0-9a-fA-F]{1,4}` | `(\\x[0-9a-fA-F]{1,4})+`
| Escaped Unicode     | `(\\u[0-9a-fA-F]{4}])\|(\\U[0-9a-fA-F]{8})` | `((\\u[0-9a-fA-F]{4}])\|(\\U[0-9a-fA-F]{8}))+`
| HexDigit            | `[a-fA-F0-9]`         | `[a-fA-F0-9]+`
| Identifier          | `[a-zA-Z0-9_]`        | `[a-zA-Z_][a-zA-Z0-9_]*`
| Letter              | `[a-zA-Z]`            | `[a-zA-Z]+`
| LowercaseIdentifier | `[a-z0-9_]`           | `[a-z_][a-z0-9_]*`
| LowercaseLetter     | `[a-z]`               | `[a-z]+`
| NewLine             | `(\r\n\|\r\|\n\|\u0085\|\u2028\|\u2029)` | `(\r\n\|\r\|\n\|\u0085\|\u2028\|\u2029)+`
| NonIdentifier       | `[^a-zA-Z0-9_]`       | `[^a-zA-Z0-9_]+`
| Number              |                       | `([0-9]*\.[0-9]+)\|([0-9][0-9]*)`
| Punctuation         | ``\\\|[!"#$%&'()*+,./:;<=>?@^_{\|}~`-]`` | ``(\\\|[!"#$%&'()*+,./:;<=>?@^_{\|}~`-])+``
| QuotedString        |                       | `("(.*(?<!\\))")\|('(.*(?<!\\))')`
| SingleQuotedString  |                       | `'(.*(?<!\\))'`
| Unicode             | `\\u(?:([0-9a-fA-F]{4})\|(?:\{((?:10[0-9a-fA-F]{4})\|(?:0?[0-9a-fA-F]{5})\|(?:[0-9a-fA-F]{1,4}))\}))` | `(\\u(?:([0-9a-fA-F]{4})\|(?:\{((?:10[0-9a-fA-F]{4})\|(?:0?[0-9a-fA-F]{5})\|(?:[0-9a-fA-F]{1,4}))\})))+`
| UppercaseIdentifier | `[A-Z0-9_]`           | `[A-Z_][A-Z0-9_]*`
| UppercaseLetter     | `[A-Z]`               | `[A-Z]+`
| Whitespace          | `\u0009\|\u000B\|\u000C\|\u0020\|\u00A0\|\u1680\|\u180E\|[\u2000-\u200A]\|\u202F\|\u3000\|\u205F` | `(\u0009\|\u000B\|\u000C\|\u0020\|\u00A0\|\u1680\|\u180E\|[\u2000-\u200A]\|\u202F\|\u3000\|\u205F)+`
