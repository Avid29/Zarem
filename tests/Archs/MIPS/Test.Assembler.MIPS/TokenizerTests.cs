// Adam Dernis 2024

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.MIPS.Helpers;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Tokenization;

namespace Test.Assembler.MIPS;

[TestClass]
public class TokenizerTests
{
    [TestMethod(TestFilePathing.EmptyTestFile)]
    public async Task EmptyFileTest() => await RunFileTest(TestFilePathing.EmptyTestFile);

    [TestMethod(TestFilePathing.InstructionsTestFile)]
    public async Task InstructionsFileTest() => await RunFileTest(TestFilePathing.InstructionsTestFile,
        "ori", "$s0", ",", "$zero", ",", "10",
        "ori", "$s1", ",", "$zero", ",", "'a'",
        "add", "$t0", ",", "$s0", ",", "$s1");

    private static async Task RunFileTest(string testFile, params string[] canon)
    {
        // Open the file and run the test
        var path = TestFilePathing.GetAssemblyFilePath(testFile);
        var stream = File.Open(path, FileMode.Open);
        await RunTest(stream, canon, testFile);
    }

    private static async Task RunTest(Stream stream, string[] canon, string? fileName = null)
    {
        // Run the test and assert the expected number of tokens came back
        var results = await Tokenizer.TokenizeAsync(stream, fileName);
        Assert.AreEqual(canon.Length, results.TokenCount);

        // Assert token strings match
        int i = 0; // Token in canon
        int j = 1; // Assembly line (1 indexed)
        int k = 0; // Token in line
        for (; i < canon.Length; i++)
        {
            // Search for next token. "while" because a line can contain no tokens
            var line = results[j];
            while (k >= line.Tokens.Length)
            {
                k = 0;
                line = results[++j];
            }

            Assert.AreEqual(canon[i], line[k++].Source);
        }
    }
}
