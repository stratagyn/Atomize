using System.Text.RegularExpressions;

namespace Atomize;

internal static class Failure
{
    public static class DidNotExpect
    {
        public static Failure<T> Char<T>(char token, int at) =>
            new(at, $"Expected not to match {{'{token}'}}.");

        public static Failure<T> Char<T>(TokenReader reader, char token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected not to match {{'{token}'}}.");

        public static Failure<T> EndOfText<T>(int at) =>
            new(at, $"Expected not to match end of text.");

        public static Failure<T> EndOfText<T>(TokenReader reader, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected not to match end of text.");

        public static Failure<T> Match<T>(TokenReader reader, int at, int start) =>
            Undo<T>(reader, at, start, "Expected not to match rule");

        public static Failure<T> Regex<T>(Regex token, int at) =>
            new(at, $"Expected not to match {{/{token}/}}.");

        public static Failure<T> Regex<T>(TokenReader reader, Regex token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected not to match {{/{token}/}}.");

        public static Failure<T> StartOfText<T>() =>
            new(0, $"Expected not to match start of text.");

        public static Failure<T> Text<T>(ReadOnlySpan<char> token, int at) =>
            new(at, $"Expected not to match {{\"{new string(token)}\"}}.");

        public static Failure<T> Text<T>(TokenReader reader, ReadOnlySpan<char> token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected not to match {{\"{new string(token)}\"}}.");
    }

    public static class Expected
    {
        public static Failure<T> Char<T>(char token, int at) =>
            new(at, $"Expected to match {{'{token}'}}.");

        public static Failure<T> Char<T>(char[] tokens, int at) =>
            new(at, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> Char<T>(TokenReader reader, char token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match {{'{token}'}}.");

        public static Failure<T> Char<T>(TokenReader reader, char[] tokens, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> EndOfText<T>(int at) =>
            new(at, $"Expected to match end of text.");

        public static Failure<T> EndOfText<T>(TokenReader reader, int at, int start) =>
            Undo<T>(reader, at, start, "Expected to match end of text.");

        public static Failure<T> NonNegativeInt<T>(int value) =>
            new(-1, $"Invalid value: {value}. Expected non-negative integer.");

        public static Failure<T> EnoughCharacters<T>(int at) =>
            new(at, "Not enough characters to read.");

        public static Failure<T> EnoughMatchingTokens<T>(int n, char token, int at) =>
            new(at, $"Failed to match {n} '{token}'.");

        public static Failure<T> EnoughMatchingTokens<T>(int n, string token, int at) =>
            new(at, $"Failed to match {n} \"{token}\".");

        public static Failure<T> EnoughMatchingTokens<T>(int n, Regex token, int at) =>
            new(at, $"Failed to match {n} /{token}/.");

        public static Failure<T> EnoughMatchingTokens<T>(TokenReader reader, int n, char token, int at, int start) =>
            Undo<T>(reader, at, start, $"Failed to match '{token}' n times.");

        public static Failure<T> EnoughMatchingTokens<T>(TokenReader reader, int n, string token, int at, int start) =>
            Undo<T>(reader, at, start, $"Failed to match \"{token}\" n times.");

        public static Failure<T> EnoughMatchingTokens<T>(TokenReader reader, int n, Regex token, int at, int start) =>
            Undo<T>(reader, at, start, $"Failed to match /{token}/ n times.");

        public static Failure<T> Regex<T>(Regex token, int at) =>
            new(at, $"Expected to match {{/{token}/}}.");

        public static Failure<T> Regex<T>(Regex[] tokens, int at) =>
            new(at, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> Regex<T>(TokenReader reader, Regex token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match {{/{token}/}}.");

        public static Failure<T> Regex<T>(TokenReader reader, Regex[] tokens, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> StartOfText<T>(int at) =>
            new(at, $"Expected to match start of text.");

        public static Failure<T> Text<T>(ReadOnlySpan<char> token, int at) =>
            new(at, $"Expected to match {{\"{new string(token)}\"}}.");

        public static Failure<T> Text<T>(TokenReader reader, ReadOnlySpan<char> token, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match {{\"{new(token)}\"}}.");

        public static Failure<T> Text<T>(TokenReader reader, string[] tokens, int at, int start) =>
            Undo<T>(reader, at, start, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> Text<T>(string[] tokens, int at) =>
            new(at, $"Expected to match one of {{{Utilities.Join(tokens)}}}.");

        public static Failure<T> ToPass<T>(char token, int at) =>
            new(at, $"Expected {token} to pass predicate.");

        public static Failure<T> ToPass<T>(string token, int at) =>
            new(at, $"Expected {token} to pass predicate.");

        public static Failure<T> ToPass<T>(T token, int at) =>
            new(at, $"Expected {token} to pass predicate.");

        public static Failure<T> ValidRange<T>(int min, int max) =>
            new(-1, $"Invalid range: [{min}, {max}].");
    }

    public static Failure<T> Undo<T>(TokenReader reader, int at, int start, string message)
    {
        reader.Backtrack(reader.Offset - start);

        return new Failure<T>(at, message);
    }
}