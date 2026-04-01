// Avishai Dernis 2024

using System.IO;
using System.Threading.Tasks;
using Test.Archs.Tokenization;
using Test.Mips.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.MIPS.Assembler;

[TestClass]
public class MipsTokenizerTests : TokenizerTester
{
    [TestMethod(TestFilePathing.EmptyTestFile)]
    public async Task EmptyFileTest() => await RunFileTest(TestFilePathing.EmptyTestFile);

    [TestMethod(TestFilePathing.InstructionsTestFile)]
    public async Task InstructionsFileTest() => await RunFileTest(TestFilePathing.InstructionsTestFile,
        ("ori", TokenType.Instruction), ("s0", TokenType.Register), (",", TokenType.Comma), ("zero", TokenType.Register), (",", TokenType.Comma), ("10", TokenType.Immediate),
        ("ori", TokenType.Instruction), ("s1", TokenType.Register), (",", TokenType.Comma), ("zero", TokenType.Register), (",", TokenType.Comma), ("'a'", TokenType.Immediate),
        ("add", TokenType.Instruction), ("t0", TokenType.Register), (",", TokenType.Comma), ("s0", TokenType.Register), (",", TokenType.Comma), ("s1", TokenType.Register));

    private static async Task RunFileTest(string testFile, params (string, TokenType)[] canon)
    {
        // Open the file and run the test
        var path = TestFilePathing.GetAssemblyFilePath(testFile);
        var stream = File.Open(path, FileMode.Open);
        await RunTest(stream, MipsTokenizerProfile.Default, canon, testFile);
    }
}
