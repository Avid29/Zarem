// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Archs;

public abstract class VersionParsingTests
{
    protected static void ParsePrintTest<T>(string input, bool reparse)
        where T : IParsable<T>
    {
        // Parse and reparse
        T info = T.Parse(input, null);
        var compare = $"{info}";

        // Reparse
        if (reparse)
        {
            input = compare;
            info = T.Parse(input, null);
            compare = $"{info}";
        }

        Assert.AreEqual(input, compare);
    }
}
