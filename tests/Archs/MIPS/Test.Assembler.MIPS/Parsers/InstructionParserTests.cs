// Avishai Dernis 2024

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Test.MIPS.Helpers;
using Zarem.Assembler.Config;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Parsers;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;

#if DEBUG
using Zarem.Disassembler.Services;
using Zarem.Services;
#endif

namespace Test.Assembler.MIPS.Parsers;

[TestClass]
public class InstructionParserTests
{
    public sealed record InstructionParsingTestCase(
        string Input,
        MIPSInstruction? Expected,
        LogId? Code)
    {
        public InstructionParsingTestCase(string input, MIPSInstruction expected) : this(input, expected, null)
        {
        }

        public InstructionParsingTestCase(string input, LogId code) : this(input, null, code)
        {
        }

        public override string ToString() => Input;
    }

    public static string InstructionParsingTestCaseDisplayName(MethodInfo _, object[] data)
        => $"{(InstructionParsingTestCase)data[0]}";

    public static IEnumerable<object[]> RawInstructionSuccessTestsList
    {
        get
        {
            yield return [new InstructionParsingTestCase("nop", MIPSInstruction.NOP)];
            yield return [new InstructionParsingTestCase("add $t0, $s0, $s1", MIPSInstruction.Create(FunctionCode.Add, GPRegister.Saved0, GPRegister.Saved1, GPRegister.Temporary0))];
            yield return [new InstructionParsingTestCase("addi $t0, $s0, 100", MIPSInstruction.Create(OperationCode.AddImmediate, GPRegister.Saved0, GPRegister.Temporary0, (short)100))];
            yield return [new InstructionParsingTestCase("sll $t0, $s0, 3", MIPSInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Saved0, GPRegister.Temporary0, 3))];
            yield return [new InstructionParsingTestCase("lw $t0, 100($s0)", MIPSInstruction.Create(OperationCode.LoadWord, GPRegister.Saved0, GPRegister.Temporary0, (short)100))];
            yield return [new InstructionParsingTestCase("sb $t0, -100($s0)", MIPSInstruction.Create(OperationCode.StoreByte, GPRegister.Saved0, GPRegister.Temporary0, (short)-100))];
            yield return [new InstructionParsingTestCase("j 1000", MIPSInstruction.Create(OperationCode.Jump, 1000))];
            yield return [new InstructionParsingTestCase("j 10*10", MIPSInstruction.Create(OperationCode.Jump, 10 * 10))];
            yield return [new InstructionParsingTestCase("di", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, GPRegister.Zero, 12))];
            yield return [new InstructionParsingTestCase("di $t1", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, GPRegister.Temporary1, 12))];
            yield return [new InstructionParsingTestCase("ei", CoProc0Instruction.Create(MFMC0FuncCode.EnableInterrupts, GPRegister.Zero, 12))];
            yield return [new InstructionParsingTestCase("cvt.S.D $f4, $f8", FloatInstruction.Create(FloatFuncCode.ConvertToSingle, FloatFormat.Double, FloatRegister.F8, FloatRegister.F4))];
        }
    }

    public static IEnumerable<object[]> RawInstructionFailureTestsList
    {
        get
        {
            yield return [new InstructionParsingTestCase("xkcd $t0, $s0, $s1", Zarem.Assembler.Logging.Enum.LogId.InvalidInstructionName)];
            yield return [new InstructionParsingTestCase("add $t0, $s0", Zarem.Assembler.Logging.Enum.LogId.InvalidInstructionArgCount)];
            yield return [new InstructionParsingTestCase("add $t0, $s0, $s1, $s1", Zarem.Assembler.Logging.Enum.LogId.InvalidInstructionArgCount)];
        }
    }

    public static IEnumerable<object[]> RawInstructionWarningTestsList
    {
        get
        {
            yield return [new InstructionParsingTestCase("sll $t0, $s0, 33", MIPSInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Saved0, GPRegister.Temporary0, 1), LogId.IntegerTruncated)];
            yield return [new InstructionParsingTestCase("sll $t0, $s0, -1", MIPSInstruction.Create(FunctionCode.ShiftLeftLogical, GPRegister.Zero, GPRegister.Saved0, GPRegister.Temporary0, 31), LogId.IntegerTruncated)];
            yield return [new InstructionParsingTestCase("j 0x1", MIPSInstruction.Create(OperationCode.Jump, 0x1), LogId.IntegerTruncated)];
        }
    }

    public static IEnumerable<object[]> Generated_MIPS_I_List => GenerateTestList(MipsVersion.MipsI);
    public static IEnumerable<object[]> Generated_MIPS_II_List => GenerateTestList(MipsVersion.MipsII);
    public static IEnumerable<object[]> Generated_MIPS_III_List => GenerateTestList(MipsVersion.MipsIII);
    public static IEnumerable<object[]> Generated_MIPS_IV_List => GenerateTestList(MipsVersion.MipsIV);
    public static IEnumerable<object[]> Generated_MIPS_V_List => GenerateTestList(MipsVersion.MipsV);
    public static IEnumerable<object[]> Generated_MIPS_VI_List => GenerateTestList(MipsVersion.MipsVI);

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionSuccessTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(InstructionParserTests))]
    public void RawInstructionSuccessTests(InstructionParsingTestCase @case)
        => RunTest(@case.Input, new MIPSParsedInstruction(@case.Expected!.Value));

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionFailureTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(InstructionParserTests))]
    public void RawInstructionFailureTests(InstructionParsingTestCase @case)
        => RunTest(@case.Input, logCode: @case.Code);

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionWarningTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(InstructionParserTests))]
    public void RawInstructionWarningTests(InstructionParsingTestCase @case)
        => RunTest(@case.Input, new MIPSParsedInstruction(@case.Expected!.Value), @case.Code);

    private const string LoadImmediate = "li $t0, 0x10001";
    
    [TestMethod(LoadImmediate)]
    public void LoadImmediateTest()
    {
        PseudoInstruction expected = new(PseudoOp.LoadImmediate) { RT = GPRegister.Temporary0, Immediate = 0x10001 };
        RunTest(LoadImmediate, new MIPSParsedInstruction(expected));
    }

    [TestMethod("MIPS I")]
    [DynamicData(nameof(Generated_MIPS_I_List))]
    public void Generated_MIPS_I(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsI);

    [TestMethod("MIPS II")]
    [DynamicData(nameof(Generated_MIPS_II_List))]
    public void Generated_MIPS_II(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsII);

    [TestMethod("MIPS III")]
    [DynamicData(nameof(Generated_MIPS_III_List))]
    public void Generated_MIPS_III(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsIII);

    [TestMethod("MIPS IV")]
    [DynamicData(nameof(Generated_MIPS_IV_List))]
    public void Generated_MIPS_IV(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsIV);

    [TestMethod("MIPS V")]
    [DynamicData(nameof(Generated_MIPS_V_List))]
    public void Generated_MIPS_V(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsV);

    [TestMethod("MIPS VI")]
    [DynamicData(nameof(Generated_MIPS_VI_List))]
    public void Generated_MIPS_VI(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsVI);

    private void AssembleDisassembleTest(string input, MipsVersion version)
    {
        var config = new MIPSAssemblerConfig(version);
#if DEBUG
        ServiceCollection.DisassemblerService = new MIPSDisassemblerService(config);
#endif

        var table = new InstructionTable(config);
        var parser = new MIPSInstructionParser(config, null, default, null, null);

        var tokenized = Tokenizer.TokenizeLine(input, nameof(RunTest));
        var actual = parser.Parse(tokenized);

        // Validate execution
        Assert.IsNotNull(actual);

        var result = actual?.Realize()[0];
        Assert.IsTrue(result.HasValue);

#if DEBUG
        Assert.AreEqual(input, result.Value.Disassembled);
#endif
    }

    private static void RunTest(string input, MIPSParsedInstruction? expected = null, LogId? logCode = null)
    {
        bool succeeds = expected is not null;

        // Initialize parser
        var logger = new Logger();
        var parser = new MIPSInstructionParser(new MIPSAssemblerConfig(), null, default, null, logger);

        // Parse instruction
        var line = Tokenizer.TokenizeLine(input, nameof(RunTest));
        var actual = parser.Parse(line);

        // Validate results
        Assert.AreEqual(succeeds, actual is not null);
        if (succeeds)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            var expectedReal = expected.Realize();
            var actualReal = actual.Realize();

            for (int i = 0 ; i < expectedReal.Length; i++)
            {
                Assert.AreEqual(expectedReal[i], actualReal[i]);
            }
        }

        if (logCode.HasValue)
        {
            Assert.IsTrue(logger.CurrentLog[0].Code.Id == (uint)logCode.Value);
        }
    }

    private static IEnumerable<object[]> GenerateTestList(MipsVersion version)
    {
        var table = new InstructionTable(new(version));
        foreach (var instruction in table.GetInstructions())
        {
            // TODO: Disassembling pseudo instructions
            if (instruction.IsPseudoInstruction)
                continue;

            // Apply format to instruction name, if applicable
            var name = instruction.Name;
            if (name.EndsWith(".fmt"))
            {
                name = FloatFormatTable.ApplyFormat(name, ArgGenerator.RandomFormat(instruction.FloatFormats));
            }

            // Generate instruction
            StringBuilder line = new(name);
            line.Append(' ');

            foreach (var arg in instruction.ArgumentPattern)
            {
                line.Append(arg switch
                {
                    Argument.RS or Argument.RT or Argument.RD => RegistersTable.GetRegisterString(ArgGenerator.RandomRegister()),
                    Argument.FS or Argument.FT or Argument.FD => RegistersTable.GetRegisterString(ArgGenerator.RandomRegister(), RegisterSet.FloatingPoints),
                    Argument.Immediate => $"{ArgGenerator.RandomImmediate()}",
                    Argument.Offset => $"{ArgGenerator.RandomOffset()}",
                    Argument.Address => $"{ArgGenerator.RandomAddress()}",
                    Argument.AddressBase => $"{ArgGenerator.RandomImmediate()}({RegistersTable.GetRegisterString(ArgGenerator.RandomRegister())})",
                    Argument.Shift => $"{ArgGenerator.RandomShift()}",
                    Argument.FullImmediate => Random.Shared.Next(),
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
}
