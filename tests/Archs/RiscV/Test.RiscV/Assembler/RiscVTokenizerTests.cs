// Avishai Dernis 2026

using System.Threading.Tasks;
using Test.Zarem.Tokenization;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.RiscV.Assembler;

[TestClass]
public class RiscVTokenizerTests : TokenizerTester
{
    [TestMethod("addi x10, x1, 42")]
    public async Task SimpleTest() => await RunTest("addi x10, x1, 42",
        ("addi", TokenType.Instruction), ("x10", TokenType.Register), (",", TokenType.Comma), ("x1", TokenType.Register), (",", TokenType.Comma), ("42", TokenType.Immediate));

    private static async Task RunTest(string test, params (string, TokenType)[] canon) =>
        await RunTest(test, RiscVTokenizerProfile.Default, canon);
}
