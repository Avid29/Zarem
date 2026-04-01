// Avishai Dernis 2024

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;

namespace Test.Zarem.Assembler.Parser;

[TestClass]
public class StringParserTests
{
    [TestMethod("Empty")]
    public void EmptyTest() => RunStringTest("", null);

    [TestMethod("''")]
    public void LiteralEmptyCharTest() => RunCharTest("''");

    [TestMethod("\"\"")]
    public void LiteralEmptyTest() => RunStringTest("\"\"", "");

    [TestMethod(@"\")]
    public void InvalidEscapeTest() => RunStringTest(@"\");

    [TestMethod("Hello\nWorld")]
    public void HelloWorld2LineTest() => RunStringTest("\"Hello\\nWorld\"", "Hello\nWorld");

    private static void RunStringTest(string input, string? expected = null)
        => RunTest(StringParser.TryParseString, input, expected ?? default, expected is null);

    private static void RunCharTest(string input, char? expected = null)
        => RunTest(StringParser.TryParseChar, input, expected ?? default, !expected.HasValue);

    delegate bool ParseFunc<T>(Token token, out T arg, ILogger? logger);

    private static void RunTest<T>(ParseFunc<T> func, string input, T expected, bool expectNull)
    {
        // Declare parser and attempt parsing
        var token = new Token(input);
        if (!func(token, out T actual, null))
        {
            Assert.IsTrue(expectNull);
            return;
        }

        // Assert the result matches the expected
        Assert.AreEqual(expected, actual);
    }
}
