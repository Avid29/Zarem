// Avishai Dernis 2024

using System.Reflection;
using Zarem.Assembler;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Tokenization;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Models.Instructions;

namespace Test.RiscV.Assembler;

[TestClass]
public class RiscVInstructionParserTests
{
    public sealed record RiscVInstructionParsingTestCase(
        string Input,
        RiscVInstruction? Expected,
        LogId? Code)
    {
        public RiscVInstructionParsingTestCase(string input, RiscVInstruction expected) : this(input, expected, null)
        {
        }

        public RiscVInstructionParsingTestCase(string input, LogId code) : this(input, null, code)
        {
        }

        public override string ToString() => Input;
    }

    public static string InstructionParsingTestCaseDisplayName(MethodInfo _, object[] data)
        => $"{(RiscVInstructionParsingTestCase)data[0]}";

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV32I")]
    public void InstructionTests_RV32I(string input)
        => AssembleDisassembleTest(input, "RV32I");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV32G")]
    public void InstructionTests_RV32G(string input)
        => AssembleDisassembleTest(input, "RV32G");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV32CF")]
    public void InstructionTests_RV32C(string input)
        => AssembleDisassembleTest(input, "RV32CF");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV64I")]
    public void InstructionTests_RV64I(string input)
        => AssembleDisassembleTest(input, "RV64I");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV64G")]
    public void InstructionTests_RV64G(string input)
        => AssembleDisassembleTest(input, "RV64G");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV64CD")]
    public void InstructionTests_RV64C(string input)
        => AssembleDisassembleTest(input, "RV64CD");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV128I")]
    public void InstructionTests_RV128I(string input)
        => AssembleDisassembleTest(input, "RV128I");

    [DataTestMethod]
    [RiscVInstructionParserTestDataSource("RV128G")]
    public void InstructionTests_RV128G(string input)
        => AssembleDisassembleTest(input, "RV128G");

    private void AssembleDisassembleTest(string input, string versionString)
    {
        var versionInfo = RiscVVersionInfo.Parse(versionString);
        var config = new RiscVAssemblerConfig(versionInfo);

        //#if DEBUG
        //        ServiceCollection.DisassemblerService = new MipsDisassemblerService(config);
        //#endif

        var table = new RiscVInstructionTable(config);
        var parser = new RiscVInstructionParser(config, table, default, null, null);

        var tokenized = Tokenizer.TokenizeLine(input, RiscVTokenizerProfile.Default)[0];
        var actual = parser.Parse(tokenized, out _);

        // Validate execution
        Assert.IsNotNull(actual);

        var result = actual[0];

        //#if DEBUG
        //        Assert.AreEqual(input, result.Value.Disassembled);
        //#endif
    }
}
