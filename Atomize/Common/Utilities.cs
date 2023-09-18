using System.Text.RegularExpressions;

namespace Atomize;

internal static class Utilities
{
    public static string Join(IEnumerable<char> parsers) =>
        string.Join(", ", parsers.Select(parser => $"'{parser}'"));

    public static string Join(IEnumerable<string> parsers) =>
        string.Join(", ", parsers.Select(parser => $"\"{parser}\""));

    public static string Join(IEnumerable<Regex> parsers) =>
        string.Join(", ", parsers.Select(parser => $"/{parser}/"));

    public static T[] UnitArray<T>(T item) => new T[] { item };
}