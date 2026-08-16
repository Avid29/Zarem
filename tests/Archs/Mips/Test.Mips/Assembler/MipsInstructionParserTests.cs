// Avishai Dernis 2024

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Test.Mips.Helpers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Tokenization;
using System.Linq;
using Zarem.Assembler;
using Zarem.Assembler.Models.Tables;
using Zarem.Mips.Services;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Assembler;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

#if DEBUG
using Zarem.Mips.Disassembler.Services;
#endif

namespace Test.Mips.Assembler;

[TestClass]
public class MipsInstructionParserTests
{
    public sealed record MipsInstructionParsingTestCase(
        string Input,
        MipsInstruction? Expected,
        LogId? Code)
    {
        public MipsInstructionParsingTestCase(string input, MipsInstruction expected) : this(input, expected, null)
        {
        }

        public MipsInstructionParsingTestCase(string input, LogId code) : this(input, null, code)
        {
        }

        public override string ToString() => Input;
    }

    public static string InstructionParsingTestCaseDisplayName(MethodInfo _, object[] data)
        => $"{(MipsInstructionParsingTestCase)data[0]}";

    public static IEnumerable<object[]> RawInstructionSuccessTestsList
    {
        get
        {
            yield return [new MipsInstructionParsingTestCase("nop", MipsInstruction.NOP)];
            yield return [new MipsInstructionParsingTestCase("add $t0, $s0, $s1", MipsInstruction.CreateR(FunctionCode.Add, MipsGpRegister.Saved0, MipsGpRegister.Saved1, MipsGpRegister.Temporary0))];
            yield return [new MipsInstructionParsingTestCase("addi $t0, $s0, 100", MipsInstruction.CreateI(MipsOpCode.AddImmediate, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)100))];
            yield return [new MipsInstructionParsingTestCase("sll $t0, $s0, 3", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 3))];
            yield return [new MipsInstructionParsingTestCase("lw $t0, 100($s0)", MipsInstruction.CreateI(MipsOpCode.LoadWord, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)100))];
            yield return [new MipsInstructionParsingTestCase("sb $t0, -100($s0)", MipsInstruction.CreateI(MipsOpCode.StoreByte, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)-100))];
            yield return [new MipsInstructionParsingTestCase("j 1000", MipsInstruction.CreateJ(MipsOpCode.Jump, 1000))];
            yield return [new MipsInstructionParsingTestCase("j 10*10", MipsInstruction.CreateJ(MipsOpCode.Jump, 10 * 10))];
            yield return [new MipsInstructionParsingTestCase("di", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, MipsGpRegister.Zero, 12))];
            yield return [new MipsInstructionParsingTestCase("di $t1", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, MipsGpRegister.Temporary1, 12))];
            yield return [new MipsInstructionParsingTestCase("ei", CoProc0Instruction.Create(MFMC0FuncCode.EnableInterrupts, MipsGpRegister.Zero, 12))];
            yield return [new MipsInstructionParsingTestCase("cvt.S.D $f4, $f8", MipsFloatInstruction.Create(MipsFloatFuncCode.ConvertToSingle, MipsFloatFormat.Double, MipsFloatRegister.F8, MipsFloatRegister.F4))];
        }
    }

    public static IEnumerable<object[]> RawInstructionFailureTestsList
    {
        get
        {
            // Invalid instruction name
            yield return [new MipsInstructionParsingTestCase("xkcd $t0, $s0, $s1", LogId.InvalidInstructionName)];

            // Invalid argument counts
            yield return [new MipsInstructionParsingTestCase("add $t0, $s0", LogId.InvalidInstructionArgCount)];
            yield return [new MipsInstructionParsingTestCase("add $t0, $s0, $s1, $s1", LogId.InvalidInstructionArgCount)];

            // Invalid registers
            yield return [new MipsInstructionParsingTestCase("jr $s", LogId.InvalidRegisterArgument)];
            yield return [new MipsInstructionParsingTestCase("jr $s80", LogId.InvalidRegisterArgument)];
            yield return [new MipsInstructionParsingTestCase("jr $80", LogId.InvalidRegisterArgument)];
        }
    }

    public static IEnumerable<object[]> RawInstructionWarningTestsList
    {
        get
        {
            yield return [new MipsInstructionParsingTestCase("sll $t0, $s0, 33", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 1), LogId.IntegerTruncated)];
            yield return [new MipsInstructionParsingTestCase("sll $t0, $s0, -1", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 31), LogId.IntegerTruncated)];
            yield return [new MipsInstructionParsingTestCase("j 0x1", MipsInstruction.CreateJ(MipsOpCode.Jump, 0x1), LogId.IntegerTruncated)];
        }
    }


    [DataTestMethod]
    [DynamicData(nameof(RawInstructionSuccessTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(MipsInstructionParserTests))]
    public void RawInstructionSuccessTests(MipsInstructionParsingTestCase @case)
        => RunTest(@case.Input, [@case.Expected!.Value]);

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionFailureTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(MipsInstructionParserTests))]
    public void RawInstructionFailureTests(MipsInstructionParsingTestCase @case)
        => RunTest(@case.Input, logCode: @case.Code);

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionWarningTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(MipsInstructionParserTests))]
    public void RawInstructionWarningTests(MipsInstructionParsingTestCase @case)
        => RunTest(@case.Input, [@case.Expected!.Value], @case.Code);

    private const string LoadImmediate = "li $t0, 0x10001";

    [TestMethod(LoadImmediate)]
    public void LoadImmediateTest()
    {
        MipsInstruction[] expected =
        [
            MipsInstruction.CreateI(MipsOpCode.LoadUpperImmediate, MipsGpRegister.Zero, MipsGpRegister.Temporary0, 0x0),
            MipsInstruction.CreateI(MipsOpCode.AddImmediateUnsigned, MipsGpRegister.Temporary0, MipsGpRegister.Temporary0, 0x1),
        ];
        RunTest(LoadImmediate, expected);
    }

    [DataTestMethod("MipsI")]
    [MipsInstructionParserTestDataSource("mips1")]
    public void InstructionTests_Mips1(string input)
        => AssembleDisassembleTest(input, "mips1");

    [DataTestMethod("MipsII")]
    [MipsInstructionParserTestDataSource("mips2")]
    public void InstructionTests_Mips2(string input)
        => AssembleDisassembleTest(input, "mips2");

    [DataTestMethod("MipsIII")]
    [MipsInstructionParserTestDataSource("mips3")]
    public void InstructionTests_Mips3(string input)
        => AssembleDisassembleTest(input, "mips3");

    [DataTestMethod("MipsIII_32bit")]
    [MipsInstructionParserTestDataSource("mips3_32bit")]
    public void InstructionTests_Mips3_32Bit(string input)
        => AssembleDisassembleTest(input, "mips3_32bit");

    [DataTestMethod("MipsIV")]
    [MipsInstructionParserTestDataSource("mips4")]
    public void InstructionTests_Mips4(string input)
        => AssembleDisassembleTest(input, "mips4");

    [DataTestMethod("MipsIV_32bit")]
    [MipsInstructionParserTestDataSource("mips4_32bit")]
    public void InstructionTests_Mips4_32Bit(string input)
        => AssembleDisassembleTest(input, "mips4_32bit");

    [DataTestMethod("MipsV")]
    [MipsInstructionParserTestDataSource("mips5")]
    public void InstructionTests_Mips5(string input)
        => AssembleDisassembleTest(input, "mips5");

    [DataTestMethod("MipsV_32bit")]
    [MipsInstructionParserTestDataSource("mips5_32bit")]
    public void InstructionTests_Mips5_32Bit(string input)
        => AssembleDisassembleTest(input, "mips5_32bit");

    [DataTestMethod("Mips32R1")]
    [MipsInstructionParserTestDataSource("mips32r1")]
    public void InstructionTests_Mips32R1(string input)
        => AssembleDisassembleTest(input, "mips32r1");

    [DataTestMethod("Mips64R1")]
    [MipsInstructionParserTestDataSource("mips64r1")]
    public void InstructionTests_Mips64R1(string input)
        => AssembleDisassembleTest(input, "mips64r1");

    [DataTestMethod("Mips32R2")]
    [MipsInstructionParserTestDataSource("mips32r2")]
    public void InstructionTests_Mips32R2(string input)
        => AssembleDisassembleTest(input, "mips32r2");

    [DataTestMethod("Mips64R2")]
    [MipsInstructionParserTestDataSource("mips64r2")]
    public void InstructionTests_Mips64R2(string input)
        => AssembleDisassembleTest(input, "mips64r2");

    private void AssembleDisassembleTest(string input, string versionString)
    {
        var versionInfo = MipsVersionInfo.Parse(versionString);
        var config = new MipsAssemblerConfig(versionInfo);
#if DEBUG
        ServiceCollection.DisassemblerService = new MipsDisassemblerService(config);
#endif

        var table = new MipsInstructionTable(config);
        var parser = new MipsInstructionParser(config, null, default, null, null);

        var tokenized = Tokenizer.TokenizeLine(input, MipsTokenizerProfile.Default, nameof(RunTest))[0];
        var actual = parser.Parse(tokenized, out _);

        // Validate execution
        Assert.IsNotNull(actual);
        var result = actual[0];

#if DEBUG
        Assert.AreEqual(input, result.Disassembled);
#endif
    }

    private static void RunTest(string input, MipsInstruction[]? expected = null, LogId? logCode = null)
    {
        bool succeeds = expected is not null;

        // Initialize parser
        var logger = new Logger();
        var parser = new MipsInstructionParser(new MipsAssemblerConfig(), null, default, null, logger);

        // Parse instruction
        var line = Tokenizer.TokenizeLine(input, MipsTokenizerProfile.Default, nameof(RunTest))[0];
        var actual = parser.Parse(line, out _);

        // Validate results
        Assert.AreEqual(succeeds, actual is not null);
        if (succeeds)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        if (logCode.HasValue)
        {
            Assert.IsTrue(logger.CurrentLog[0].Code.Id == (uint)logCode.Value);
        }
    }
}
