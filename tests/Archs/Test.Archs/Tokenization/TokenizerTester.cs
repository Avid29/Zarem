// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Tokenization.Profiles;

namespace Test.Archs.Tokenization;

public class TokenizerTester
{
    public static async Task RunTest(string text, ITokenizerProfile profile, (string, TokenType)[] canon, string? fileName = null)
    {
        using TextReader reader = new StringReader(text);
        await RunTest(reader, profile, canon, fileName);
    }

    public static async Task RunTest(Stream stream, ITokenizerProfile profile, (string, TokenType)[] canon, string? fileName = null)
    {
        using TextReader reader = new StreamReader(stream);
        await RunTest(reader, profile, canon, fileName);
    }

    public static async Task RunTest(TextReader reader, ITokenizerProfile profile, (string, TokenType)[] canon, string? fileName = null)
    {
        // Run the test and assert the expected number of tokens came back
        var results = await Tokenizer.TokenizeAsync(reader, profile, fileName);
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
