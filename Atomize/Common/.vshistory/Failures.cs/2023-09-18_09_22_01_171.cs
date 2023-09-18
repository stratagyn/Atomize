using System.Text.RegularExpressions;

namespace Atomize;

internal static class Failure
{
    public static class DidNotExpect
    {
        public static Failure<T> EndOfText<T>(int at) =>
            new(at, $"Expected not to match end of text.");

        public static Failure<T> Match<T>(TextScanner scanner, int at, int start) =>
            Undo<T>(scanner, at, start, "Expected not to match rule");
    }

    public static class Expected
    {
        public static Failure<T> Char<T>(char parser, int at) =>
            new(at, $"Expected to match {{'{parser}'}}.");

        public static Failure<T> EndOfText<T>(int at) =>
            new(at, $"Expected to match end of text.");

        public static Failure<T> Match<T>(TextScanner scanner, int at, int start) =>
            Undo<T>(scanner, at, start, "Expected to match rule");

        public static Failure<T> NonNegativeInt<T>(int value) =>
            new(-1, $"Invalid value: {value}. Expected non-negative integer.");

        public static Failure<T> EnoughCharacters<T>(int at) =>
            new(at, "Not enough characters to read.");

        public static Failure<T> EnoughMatchingTokens<T>(int n, char parser, int at) =>
            new(at, $"Failed to match {n} '{parser}'.");

        public static Failure<T> EnoughMatchingTokens<T>(int n, string parser, int at) =>
            new(at, $"Failed to match {n} \"{parser}\".");

        public static Failure<T> EnoughMatchingTokens<T>(int n, Regex parser, int at) =>
            new(at, $"Failed to match {n} /{parser}/.");

        public static Failure<T> EnoughMatchingTokens<T>(TextScanner scanner, int n, char parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Failed to match '{parser}' {n} times.");

        public static Failure<T> EnoughMatchingTokens<T>(TextScanner scanner, int n, string parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Failed to match \"{parser}\" {n} times.");

        public static Failure<T> EnoughMatchingTokens<T>(TextScanner scanner, int n, Regex parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Failed to match /{parser}/ {n} times.");

        public static Failure<T> EnoughMatchingTokens<T>(TextScanner scanner, int n, int at, int start) =>
            Undo<T>(scanner, at, start, $"Failed to match rule {n} times.");

        public static Failure<T> Regex<T>(Regex parser, int at) =>
            new(at, $"Expected to match {{/{parser}/}}.");

        public static Failure<T> Regex<T>(Regex[] parsers, int at) =>
            new(at, $"Expected to match one of {{{Utilities.Join(parsers)}}}.");

        public static Failure<T> Regex<T>(TextScanner scanner, Regex parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected to match {{/{parser}/}}.");

        public static Failure<T> Regex<T>(TextScanner scanner, Regex[] parsers, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected to match one of {{{Utilities.Join(parsers)}}}.");

        public static Failure<T> StartOfText<T>(int at) =>
            new(at, $"Expected to match start of text.");

        public static Failure<T> Text<T>(ReadOnlySpan<char> parser, int at) =>
            new(at, $"Expected to match {{\"{new string(parser)}\"}}.");

        public static Failure<T> Text<T>(TextScanner scanner, ReadOnlySpan<char> parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected to match {{\"{new(parser)}\"}}.");

        public static Failure<T> Text<T>(TextScanner scanner, string[] parsers, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected to match one of {{{Utilities.Join(parsers)}}}.");

        public static Failure<T> Text<T>(string[] parsers, int at) =>
            new(at, $"Expected to match one of {{{Utilities.Join(parsers)}}}.");

        public static Failure<T> ToPass<T>(char parser, int at) =>
            new(at, $"Expected {parser} to pass predicate.");

        public static Failure<T> ToPass<T>(TextScanner scanner, char parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected {parser} to pass predicate.");

        public static Failure<T> ToPass<T>(string parser, int at) =>
            new(at, $"Expected {parser} to pass predicate.");

        public static Failure<T> ToPass<T>(TextScanner scanner, string parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected {parser} to pass predicate.");

        public static Failure<T> ToPass<T>(T parser, int at) =>
            new(at, $"Expected {parser} to pass predicate.");

        public static Failure<T> ToPass<T>(TextScanner scanner, T parser, int at, int start) =>
            Undo<T>(scanner, at, start, $"Expected {parser} to pass predicate.");

        public static Failure<T> ValidRange<T>(int min, int max) =>
            new(-1, $"Invalid range: [{min}, {max}].");
    }

    public static Failure<T> Undo<T>(TextScanner scanner, int at, int start, string message)
    {
        scanner.Backtrack(scanner.Offset - start);

        return new Failure<T>(at, message);
    }
}