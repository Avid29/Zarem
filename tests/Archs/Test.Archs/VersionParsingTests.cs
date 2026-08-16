// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Archs;

public abstract class VersionParsingTests
{
    protected static void ParsePrintTest<T>(string input)
        where T : IParsable<T>
    {
        // Parse and reparse
        T info = T.Parse(input, null);
        var compare = $"{info}";

        // Reparse
        info = T.Parse(compare, null);
        Assert.AreEqual(compare, $"{info}");
    }
}
