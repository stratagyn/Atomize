using System.Text.RegularExpressions;

namespace Atomize.Tests;

public partial class AtomizeTests
{
    private const string TestText = "abcdefghijklmnopqrstuvwxyz0123456789";

    private const int TestOffset = 26;

    private static readonly Regex Pattern.Char.LowercaseLetter = Pattern.Char.LowercaseLetterRegex();
    private static readonly Regex Number = NumberRegex();
    private static readonly Regex Uppercase = UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex Pattern.Char.LowercaseLetterRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex NumberRegex();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();
}