// Avishai Dernis 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Test.RiscV.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Tokenization;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Test.RiscV.Assembler.Parsers;

[TestClass]
public class InstructionParserTests
{
    public sealed record InstructionParsingTestCase(
        string Input,
        RiscVInstruction? Expected,
        LogId? Code)
    {
        public InstructionParsingTestCase(string input, RiscVInstruction expected) : this(input, expected, null)
        {
        }

        public InstructionParsingTestCase(string input, LogId code) : this(input, null, code)
        {
        }

        public override string ToString() => Input;
    }

    public static string InstructionParsingTestCaseDisplayName(MethodInfo _, object[] data)
        => $"{(InstructionParsingTestCase)data[0]}";

    public static IEnumerable<object[]> Generated_RV32_I_List => GenerateTestList(new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.Integers));

    public static IEnumerable<object[]> Generated_RV64_I_List => GenerateTestList(new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.Integers));

    public static IEnumerable<object[]> Generated_RV32_G_List => GenerateTestList(new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.General));

    public static IEnumerable<object[]> Generated_RV64_G_List => GenerateTestList(new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.General));

    [TestMethod("RV32-I")]
    [DynamicData(nameof(Generated_RV32_I_List))]
    public void Generated_RV32_I(string input)
        => AssembleDisassembleTest(input, new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.Integers));

    [TestMethod("RV64-I")]
    [DynamicData(nameof(Generated_RV64_I_List))]
    public void Generated_RV64_I(string input)
        => AssembleDisassembleTest(input, new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.Integers));

    [TestMethod("RV32-G")]
    [DynamicData(nameof(Generated_RV32_G_List))]
    public void Generated_RV32_G(string input)
        => AssembleDisassembleTest(input, new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.General));

    [TestMethod("RV64-G")]
    [DynamicData(nameof(Generated_RV64_G_List))]
    public void Generated_RV64_G(string input)
        => AssembleDisassembleTest(input, new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.General));

    private void AssembleDisassembleTest(string input, RiscVVersionInfo version)
    {
        var config = new RiscVAssemblerConfig(version);
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

    //private static void RunTest(string input, MipsParsedInstruction? expected = null, LogId? logCode = null)
    //{
    //    bool succeeds = expected is not null;

    //    // Initialize parser
    //    var logger = new Logger();
    //    var parser = new RiscVInstructionParser(new RiscVAssemblerConfig(), null, default, null, logger);

    //    // Parse instruction
    //    var line = Tokenizer.TokenizeLine(input, RiscVTokenizerProfile.Default, nameof(RunTest))[0];
    //    //var actual = parser.Parse(line);

    //    // Validate results
    //    Assert.AreEqual(succeeds, actual is not null);
    //    if (succeeds)
    //    {
    //        Assert.IsNotNull(expected);
    //        Assert.IsNotNull(actual);

    //        var expectedReal = expected.Realize();
    //        var actualReal = actual.Realize();

    //        for (int i = 0 ; i < expectedReal.Length; i++)
    //        {
    //            Assert.AreEqual(expectedReal[i], actualReal[i]);
    //        }
    //    }

    //    if (logCode.HasValue)
    //    {
    //        Assert.IsTrue(logger.CurrentLog[0].Code.Id == (uint)logCode.Value);
    //    }
    //}

    private static IEnumerable<object[]> GenerateTestList(RiscVVersionInfo version)
    {
        var table = new RiscVInstructionTable(new(version));
        var instructions = table.GetInstructions()
            .Where(i => i.IsValidFor(version));

        foreach (var instruction in instructions)
        {
            // TODO: Disassembling pseudo instructions
            if (instruction is RiscVPseudoInstructionMeta)
                continue;

            // Generate instruction
            StringBuilder line = new(instruction.Name);
            line.Append(' ');

            foreach (var arg in instruction.ArgumentPattern)
            {
                line.Append(arg switch
                {
                    RiscVArgument.RD or RiscVArgument.RS1 or RiscVArgument.RS2 => GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.GeneralPurpose),
                    RiscVArgument.FRD or RiscVArgument.FRS1 or RiscVArgument.FRS2 or RiscVArgument.FRS3 => GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.FloatingPoints),
                    RiscVArgument.Immediate or RiscVArgument.StoreOffset or RiscVArgument.Csr=> $"{ArgGenerator.RandomImm12()}",
                    RiscVArgument.UpperImmediate => $"{ArgGenerator.RandomImm20()}",
                    RiscVArgument.BranchOffset => $"{ArgGenerator.RandomBranchOffset()}",
                    RiscVArgument.JumpOffset => $"{ArgGenerator.RandomJumpOffset()}",
                    RiscVArgument.FullImmediate => $"{ArgGenerator.RandomFullImm()}",
                    RiscVArgument.UImm5 => $"{ArgGenerator.RandomShamt()}",
                    RiscVArgument.MemoryLoad or RiscVArgument.MemoryStore => $"{ArgGenerator.RandomImm12()}({GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.GeneralPurpose)})",
                    _ => throw new NotImplementedException(),
                });

                line.Append(", ");
            }

            // Remove final ", "
            if (instruction.ArgumentPattern.Length > 0)
                line.Remove(line.Length - 2, 2);

            // Return test case
            yield return [$"{line}"];
        }
    }

    private static string GetRegisterString(RiscVGpRegister register, RiscVRegisterSet set) => $"{RiscVRegisterTable.Instance.GetRegisterString(register, set)}";
}
