using System.Text.RegularExpressions;

namespace Atomize.Tests;

public partial class AtomizeTests
{
    private const string TestText = "abcdefghijklmnopqrstuvwxyz0123456789";

    private const int TestOffset = 26;

    private static readonly Regex Lowercase = Lowercasse();

    [GeneratedRegex("[a-z]")]
    private static partial Regex Lowercasse();
}