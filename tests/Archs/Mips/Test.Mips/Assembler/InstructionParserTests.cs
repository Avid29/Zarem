// Avishai Dernis 2024

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Test.Mips.Helpers;
using Zarem.Assembler.Helpers.Tables;
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

#if DEBUG
using Zarem.Mips.Disassembler.Services;
#endif

namespace Test.Mips.Assembler;

[TestClass]
public class InstructionParserTests
{
    public sealed record InstructionParsingTestCase(
        string Input,
        MipsInstruction? Expected,
        LogId? Code)
    {
        public InstructionParsingTestCase(string input, MipsInstruction expected) : this(input, expected, null)
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
            yield return [new InstructionParsingTestCase("nop", MipsInstruction.NOP)];
            yield return [new InstructionParsingTestCase("add $t0, $s0, $s1", MipsInstruction.CreateR(FunctionCode.Add, MipsGpRegister.Saved0, MipsGpRegister.Saved1, MipsGpRegister.Temporary0))];
            yield return [new InstructionParsingTestCase("addi $t0, $s0, 100", MipsInstruction.CreateI(MipsOpCode.AddImmediate, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)100))];
            yield return [new InstructionParsingTestCase("sll $t0, $s0, 3", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 3))];
            yield return [new InstructionParsingTestCase("lw $t0, 100($s0)", MipsInstruction.CreateI(MipsOpCode.LoadWord, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)100))];
            yield return [new InstructionParsingTestCase("sb $t0, -100($s0)", MipsInstruction.CreateI(MipsOpCode.StoreByte, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, (short)-100))];
            yield return [new InstructionParsingTestCase("j 1000", MipsInstruction.CreateJ(MipsOpCode.Jump, 1000))];
            yield return [new InstructionParsingTestCase("j 10*10", MipsInstruction.CreateJ(MipsOpCode.Jump, 10 * 10))];
            yield return [new InstructionParsingTestCase("di", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, MipsGpRegister.Zero, 12))];
            yield return [new InstructionParsingTestCase("di $t1", CoProc0Instruction.Create(MFMC0FuncCode.DisableInterrupts, MipsGpRegister.Temporary1, 12))];
            yield return [new InstructionParsingTestCase("ei", CoProc0Instruction.Create(MFMC0FuncCode.EnableInterrupts, MipsGpRegister.Zero, 12))];
            yield return [new InstructionParsingTestCase("cvt.S.D $f4, $f8", MipsFloatInstruction.Create(MipsFloatFuncCode.ConvertToSingle, MipsFloatFormat.Double, MipsFloatRegister.F8, MipsFloatRegister.F4))];
        }
    }

    public static IEnumerable<object[]> RawInstructionFailureTestsList
    {
        get
        {
            // Invalid instruction name
            yield return [new InstructionParsingTestCase("xkcd $t0, $s0, $s1", LogId.InvalidInstructionName)];

            // Invalid argument counts
            yield return [new InstructionParsingTestCase("add $t0, $s0", LogId.InvalidInstructionArgCount)];
            yield return [new InstructionParsingTestCase("add $t0, $s0, $s1, $s1", LogId.InvalidInstructionArgCount)];

            // Invalid registers
            //yield return [new InstructionParsingTestCase("jr $s", LogId.InvalidRegisterArgument)];
            //yield return [new InstructionParsingTestCase("jr $s80", LogId.InvalidRegisterArgument)];
            //yield return [new InstructionParsingTestCase("jr $80", LogId.InvalidRegisterArgument)];
        }
    }

    public static IEnumerable<object[]> RawInstructionWarningTestsList
    {
        get
        {
            yield return [new InstructionParsingTestCase("sll $t0, $s0, 33", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 1), LogId.IntegerTruncated)];
            yield return [new InstructionParsingTestCase("sll $t0, $s0, -1", MipsInstruction.CreateR(FunctionCode.ShiftLeftLogical, MipsGpRegister.Zero, MipsGpRegister.Saved0, MipsGpRegister.Temporary0, 31), LogId.IntegerTruncated)];
            yield return [new InstructionParsingTestCase("j 0x1", MipsInstruction.CreateJ(MipsOpCode.Jump, 0x1), LogId.IntegerTruncated)];
        }
    }

    public static IEnumerable<object[]> Generated_MIPS_I_List => GenerateTestList(MipsVersion.MipsI);
    public static IEnumerable<object[]> Generated_MIPS_II_List => GenerateTestList(MipsVersion.MipsII);
    public static IEnumerable<object[]> Generated_MIPS_III_List => GenerateTestList(MipsVersion.MipsIII);
    public static IEnumerable<object[]> Generated_MIPS_III_32Bit_List => GenerateTestList(MipsVersion.MipsIII_32Bit);
    public static IEnumerable<object[]> Generated_MIPS_IV_List => GenerateTestList(MipsVersion.MipsIV);
    public static IEnumerable<object[]> Generated_MIPS_IV_32Bit_List => GenerateTestList(MipsVersion.MipsIV_32Bit);
    public static IEnumerable<object[]> Generated_MIPS_V_List => GenerateTestList(MipsVersion.MipsV);
    public static IEnumerable<object[]> Generated_MIPS_V_32Bit_List => GenerateTestList(MipsVersion.MipsV_32Bit);
    public static IEnumerable<object[]> Generated_MIPS32_R1_List => GenerateTestList(MipsVersion.Mips32R1);
    public static IEnumerable<object[]> Generated_MIPS32_R2_List => GenerateTestList(MipsVersion.Mips32R2);
    public static IEnumerable<object[]> Generated_MIPS32_R6_List => GenerateTestList(MipsVersion.Mips32R6);

    [DataTestMethod]
    [DynamicData(nameof(RawInstructionSuccessTestsList),
        DynamicDataDisplayName = nameof(InstructionParsingTestCaseDisplayName),
        DynamicDataDisplayNameDeclaringType = typeof(InstructionParserTests))]
    public void RawInstructionSuccessTests(InstructionParsingTestCase @case)
        => RunTest(@case.Input, [@case.Expected!.Value]);

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
        => RunTest(@case.Input, [@case.Expected!.Value], @case.Code);

    private const string LoadImmediate = "li $t0, 0x10001";

    [TestMethod(LoadImmediate)]
    public void LoadImmediateTest()
    {
        MipsInstruction[] expected =
        [
            MipsInstruction.CreateI(MipsOpCode.LoadUpperImmediate, MipsGpRegister.Zero, MipsGpRegister.Temporary0, 0x0),
            MipsInstruction.CreateI(MipsOpCode.OrImmediate, MipsGpRegister.Temporary0, MipsGpRegister.Temporary0, 0x1),
        ];
        RunTest(LoadImmediate, expected);
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

    [TestMethod("MIPS III (32 Bit)")]
    [DynamicData(nameof(Generated_MIPS_III_32Bit_List))]
    public void Generated_MIPS_III_32Bit(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsIII_32Bit);

    [TestMethod("MIPS IV")]
    [DynamicData(nameof(Generated_MIPS_IV_List))]
    public void Generated_MIPS_IV(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsIV);

    [TestMethod("MIPS IV (32 Bit)")]
    [DynamicData(nameof(Generated_MIPS_IV_32Bit_List))]
    public void Generated_MIPS_IV_32Bit(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsIV_32Bit);

    [TestMethod("MIPS V")]
    [DynamicData(nameof(Generated_MIPS_V_List))]
    public void Generated_MIPS_V(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsV);

    [TestMethod("MIPS V (32 Bit)")]
    [DynamicData(nameof(Generated_MIPS_V_32Bit_List))]
    public void Generated_MIPS_V_32Bit(string input)
        => AssembleDisassembleTest(input, MipsVersion.MipsV_32Bit);

    [TestMethod("MIPS32 Release 1")]
    [DynamicData(nameof(Generated_MIPS32_R1_List))]
    public void Generated_MIPS32_R1(string input)
        => AssembleDisassembleTest(input, MipsVersion.Mips32R1);

    [TestMethod("MIPS64 Release 1")]
    [DynamicData(nameof(Generated_MIPS32_R1_List))]
    public void Generated_MIPS64_R1(string input)
        => AssembleDisassembleTest(input, MipsVersion.Mips64R1);

    [TestMethod("MIPS32 Release 2")]
    [DynamicData(nameof(Generated_MIPS32_R2_List))]
    public void Generated_MIPS32_R2(string input)
        => AssembleDisassembleTest(input, MipsVersion.Mips32R2);

    [TestMethod("MIPS32 Release 2")]
    [DynamicData(nameof(Generated_MIPS32_R2_List))]
    public void Generated_MIPS64_R2(string input)
        => AssembleDisassembleTest(input, MipsVersion.Mips64R2);

    //[TestMethod("MIPS32 R6")]
    //[DynamicData(nameof(Generated_MIPS32_R6_List))]
    //public void Generated_MIPS32_R6(string input)
    //    => AssembleDisassembleTest(input, MipsVersion.Mips32R6);

    private void AssembleDisassembleTest(string input, MipsVersion version)
    {
        var config = new MipsAssemblerConfig(version);
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

            for ( var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        if (logCode.HasValue)
        {
            Assert.IsTrue(logger.CurrentLog[0].Code.Id == (uint)logCode.Value);
        }
    }

    private static IEnumerable<object[]> GenerateTestList(MipsVersion version)
    {
        var formatTable = new FormatTable<MipsFloatFormat>();
        var table = new MipsInstructionTable(new(version));
        var instructions = table.GetInstructions()
            .Where(i => i.IsValidFor(version));

        foreach (var instruction in instructions)
        {
            // TODO: Disassembling pseudo instructions
            if (instruction is MipsPseudoInstructionMeta)
                continue;

            // Apply format to instruction name, if applicable
            var name = instruction.Name;
            if (instruction is MipsFloatInstructionMeta fMeta)
            {
                name = formatTable.ApplyFormat(name, ArgGenerator.RandomFormat(fMeta.SupportedFormats));
            }

            // Generate instruction
            StringBuilder line = new(name);
            if (instruction.ArgumentCount is not 0)
                line.Append(' ');

            foreach (var arg in instruction.ArgumentPattern)
            {
                line.Append(arg switch
                {
                    MipsArgument.RS or MipsArgument.RT or MipsArgument.RD => GetRegisterString(ArgGenerator.RandomRegister(), MipsRegisterSet.GeneralPurpose),
                    MipsArgument.FS or MipsArgument.FT or MipsArgument.FD => GetRegisterString(ArgGenerator.RandomRegister(), MipsRegisterSet.FloatingPoints),
                    MipsArgument.Immediate => $"{ArgGenerator.RandomImmediate()}",
                    MipsArgument.Offset => $"{ArgGenerator.RandomOffset()}",
                    MipsArgument.LargeOffset => $"{ArgGenerator.RandomOffset()}",
                    MipsArgument.Address => $"{ArgGenerator.RandomAddress()}",
                    MipsArgument.AddressBase => $"{ArgGenerator.RandomImmediate()}({GetRegisterString(ArgGenerator.RandomRegister(), MipsRegisterSet.GeneralPurpose)})",
                    MipsArgument.ShiftAmount => $"{ArgGenerator.RandomShift()}",
                    MipsArgument.FullImmediate => Random.Shared.Next(),
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

    private static string GetRegisterString(MipsGpRegister register, MipsRegisterSet set) => $"${MipsRegisterTable.Instance.GetRegisterString(register, set)}";
}
