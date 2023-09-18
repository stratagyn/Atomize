using System.Text.RegularExpressions;

namespace Atomize;

internal static class Utilities
{
    public static string Join(IEnumerable<char> tokens) =>
        string.Join(", ", tokens.Select(token => $"'{token}'"));

    public static string Join(IEnumerable<string> tokens) =>
        string.Join(", ", tokens.Select(token => $"\"{token}\""));

    public static string Join(IEnumerable<Regex> tokens) =>
        string.Join(", ", tokens.Select(token => $"/{token}/"));

    public static T[] UnitArray<T>(T item) => new T[] { item };
}