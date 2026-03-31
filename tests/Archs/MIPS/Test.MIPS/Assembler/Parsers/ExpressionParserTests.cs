// Avishai Dernis 2024

using System.Collections.Generic;
using System.Numerics;
using Zarem.Assembler;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.MIPS.Assembler.Parsers;

[TestClass]
public class ExpressionParserTests
{
    public static IEnumerable<object[]> IntegerSuccessTestsList =>
    [
        ["10", 10],
        ["-10", -10],
        ["10", 10],
        ["+10", 10],
        ["25 - 10", 25 - 10],
        ["4 * 4", 4 * 4],
        ["8 / 2", 8 / 2],
        ["8 % 3", 8 % 3],
        ["9 & 3", 9 & 3],
        ["9 | 3", 9 | 3],
        ["9 ^ 3", 9 ^ 3],
        ["~10", ~10],
        ["10 * -10", 10 * -10],
        ["0b1010", 0b1010],
        ["0b10_10", 0b10_10],
        ["0o12", 10], // C# Doesn't support oct
        ["0xa", 0xa],
        ["0xff_00", 0xff_00],
        ["4 * 2 + 2", 4 * 2 + 2],
        ["4 + 2 * 2", 4 + 2 * 2],
        ["(4 + 2) * 2", (4 + 2) * 2],
        ["'a'", 'a'],
        [@"'\n'", '\n'],
        [@"'\\'", '\\'],
        [@"'\0'", '\0'],
        ["'a' + 10", 'a' + 10],
    ];

    public static IEnumerable<object[]> FloatSuccessTestsList =>
    [
        ["10", 10d],
        ["-10", -10d],
        ["1.0", 1.0],
        ["1.4", 1.4],
        ["0.4", 0.4],
        ["-1.0", -1.0],
        ["-1.4", -1.4],
        ["2 - 1.4", 2 - 1.4],
    ];

    public static IEnumerable<object[]> IntegerFailureTestsList =>
    [
        ["+"],
        ["*10"],
        ["10-"],
        ["-*10"],
        ["10 10"],
        ["0b102"],
        ["0o109"],
        ["_0xFF"],
        ["0xFF_"],
        ["4 + 2) * 2"],
        ["(4 + 2 * 2"],
        ["'abc'"],
        [@"'\x'"],
        [@"3.0"],
        [@"3.2"],
    ];

    [DataTestMethod]
    [DynamicData(nameof(IntegerSuccessTestsList))]
    public void IntegerSuccessTests(string input, long expected)
        => RunTest<long>(input, expected);

    [DataTestMethod]
    [DynamicData(nameof(FloatSuccessTestsList))]
    public void FloatSuccessTests(string input, double expected)
        => RunTest<double>(input, expected);

    [DataTestMethod]
    [DynamicData(nameof(IntegerFailureTestsList))]
    public void IntegerFailureTests(string input)
        => RunTest<long>(input);

    private static void RunTest<T>(string input, T? expected = null)
        where T : unmanaged, IBinaryNumber<T>
    {
        var line = Tokenizer.TokenizeLine(input, MipsTokenizerProfile.Default, nameof(RunTest), TokenizerMode.Expression)[0];
        bool success = ExpressionParser.TryParse<T>(line.Tokens, out var actual, null, null);
        Assert.AreEqual(success, expected.HasValue);
        if (expected.HasValue)
        {
            Assert.AreEqual(expected.Value, actual.Addend);
        }
    }
}
