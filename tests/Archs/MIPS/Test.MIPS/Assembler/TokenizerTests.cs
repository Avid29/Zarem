// Avishai Dernis 2024

using System.IO;
using System.Threading.Tasks;
using Test.Mips.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Test.MIPS.Assembler
{
    [TestClass]
    public class TokenizerTests
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
            await RunTest(stream, canon, testFile);
        }

        private static async Task RunTest(Stream stream, (string, TokenType)[] canon, string? fileName = null)
        {
            // Run the test and assert the expected number of tokens came back
            var results = await Tokenizer.TokenizeAsync(stream, MipsTokenizerProfile.Default, fileName);
            Assert.AreEqual(canon.Length, results.TokenCount);

            // Assert token strings match
            int i = 0; // Token in canon
            int j = 0; // Assembly line
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

                Assert.AreEqual(canon[i].Item1, line[k].Source);
                Assert.AreEqual(canon[i].Item2, line[k].Type);
                k++;
            }
        }
    }
}
