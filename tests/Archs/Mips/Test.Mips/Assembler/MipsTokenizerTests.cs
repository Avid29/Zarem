// Avishai Dernis 2024

using System.Threading.Tasks;
using Test.Archs.Tokenization;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.Mips.Assembler;

[TestClass]
public class MipsTokenizerTests : TokenizerTester
{
    [TestMethod("Empty")]
    public async Task EmptyTest() => await RunTest("", []);

    [TestMethod("ori $s0, $zero, 10")]
    public async Task SimpleTest()
    {
        var tokensBuilder = new TokenExpectationBuilder(MipsTokenizerProfile.Default)
            .Instruction("ori").Reg("s0").Comma().Reg("zero").Comma().Imm("10");

        await RunTest("ori $s0, $zero, 10", tokensBuilder.Build());
    }

    private static async Task RunTest(string text, params (string, TokenType)[] canon) => await RunTest(text, MipsTokenizerProfile.Default, canon);
}
