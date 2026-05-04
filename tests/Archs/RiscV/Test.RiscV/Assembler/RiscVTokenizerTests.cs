// Avishai Dernis 2026

using System.Threading.Tasks;
using Test.Archs.Tokenization;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.RiscV.Assembler;

[TestClass]
public class RiscVTokenizerTests : TokenizerTester
{
    [TestMethod("Empty")]
    public async Task EmptyTest() => await RunTest("", []);

    [TestMethod("addi x10, x1, 42")]
    public async Task SimpleTest()
    {
        var tokensBuilder = new TokenExpectationBuilder(MipsTokenizerProfile.Default)
            .Instruction("addi").Reg("x10").Comma().Reg("x1").Comma().Imm("42");

        await RunTest("addi x10, x1, 42", tokensBuilder.Build());
    }

    private static async Task RunTest(string test, params (string, TokenType)[] canon) =>
        await RunTest(test, RiscVTokenizerProfile.Default, canon);
}
