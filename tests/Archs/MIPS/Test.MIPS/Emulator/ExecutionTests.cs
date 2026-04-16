// Avishai Dernis 2026

using System.Numerics;
using System.Xml;
using Zarem.Assembler;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interpret;
using Zarem.Emulator.Machine.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.MIPS.Emulator;

[TestClass]
public partial class ExecutionTests
{
    public const uint K0 = 0xbd0;
    public const uint K1 = 0xd16;

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips1))]
    public void InstructionTests_Mips1(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsI);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips1))]
    public void InstructionTests_Mips1_JIT(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsI, true);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips2))]
    public void InstructionTests_Mips2(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsII);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips3))]
    public void InstructionTests_Mips3(ExecutionTestCase<ulong> @case) => RunTest(@case, MipsVersion.MipsIII);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips3_32Bit))]
    public void InstructionTests_Mips3_32Bit(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsIII_32Bit);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips4))]
    public void InstructionTests_Mips4(ExecutionTestCase<ulong> @case) => RunTest(@case, MipsVersion.MipsIV);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips4_32Bit))]
    public void InstructionTests_Mips4_32Bit(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsIV_32Bit);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips5))]
    public void InstructionTests_Mips5(ExecutionTestCase<ulong> @case) => RunTest(@case, MipsVersion.MipsV);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips5_32Bit))]
    public void InstructionTests_Mips5_32Bit(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.MipsV_32Bit);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips32R1))]
    public void InstructionTests_Mips32R1(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.Mips32R1);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips64R1))]
    public void InstructionTests_Mips64R1(ExecutionTestCase<ulong> @case) => RunTest(@case, MipsVersion.Mips64R1);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips32R2))]
    public void InstructionTests_Mips32R2(ExecutionTestCase<uint> @case) => RunTest(@case, MipsVersion.Mips32R2);

    [DataTestMethod]
    [DynamicData(nameof(InstructionTestList_Mips64R2))]
    public void InstructionTests_Mips64R2(ExecutionTestCase<ulong> @case) => RunTest(@case, MipsVersion.Mips64R2);

    private static void RunTest<T>(ExecutionTestCase<T> @case, MipsVersion version, bool jit = false)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var config = new MipsEmulatorConfig(version)
        {
            DisableDelaySlots = false,
            ExecutionMode = jit ? ExecutionMode.JustInTime : ExecutionMode.Interpret,
        };

        // Run with delay slots by default
        RunTest(@case, config);

        // Run again without if jump/branch instruction
        if (@case.ExpectedPC.HasValue)
        {
            config.DisableDelaySlots = true;
            RunTest(@case, config);
        }
    }
    
    private static void RunTest<T>(ExecutionTestCase<T> @case, MipsEmulatorConfig config)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input, MipsTokenizerProfile.Default)[0];
        var table = new MipsInstructionTable(new MipsAssemblerConfig(config.Version));
        var parser = new MipsInstructionParser(new(), table, default, null, null);
        var parsed = parser.Parse(tokenized);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed.Realize()[0];
        var computer = new MipsComputer(config);
        var cpu = (MipsCpu<T>)computer.Cpu;

        // Initialize the status register
        cpu.CoProcessor0.StatusRegister = @case.Status;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            cpu[reg] = value;

        foreach (var (reg, value) in @case.FPRInitialization)
        {
            cpu.FloatProcessor[reg] = value;
        }

        // Initialize the high and low registers if specified in the test case
        if (@case.InitialHighLow.HasValue)
        {
            cpu.RegisterFile.Low = @case.InitialHighLow.Value.Low;
            cpu.RegisterFile.High = @case.InitialHighLow.Value.High;
        }

        // Initialize the memory, if specified in the test case
        foreach (var (address, data) in @case.MemoryInitialization)
            computer.Memory.Write(ulong.CreateTruncating(address), data);

        if (cpu is MipsJitCpu<T>)
        {
            RunJitChecks(computer, instruction, @case);
        }
        else if (cpu is MipsInterpretCpu<T>)
        {
            RunInterpretChecks(computer, instruction, @case);
        }
    }

    private static void RunInterpretChecks<T>(MipsComputer computer, MipsInstruction instruction, ExecutionTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var cpu = (MipsInterpretCpu<T>)computer.Cpu;
        cpu.Insert(instruction, out var execution, out var trap);

        // Ensure that the expected trap was raised (if any)
        Assert.AreEqual(@case.ExpectedTrap, trap);

        var writeback = @case.ExpectedWriteBack;
        if (writeback.HasValue)
        {
            // Ensure that the expected register was written to with the expected value
            Assert.AreEqual(writeback.Value.Register, execution.GPR);

            var writeBackValue = writeback.Value.Value;
            if (writeBackValue.HasValue)
            {
                Assert.AreEqual(writeBackValue.Value, cpu[execution.GPR]);
            }
        }
        else
        {
            // If no register check was provided, we at least want to make sure no register was written to (as that would be unexpected)
            Assert.AreEqual(MipsGpRegister.Zero, execution.GPR);
        }

        var highLow = @case.ExpectedHighLow;
        if (highLow.HasValue)
        {
            Assert.AreEqual(highLow.Value.Low, cpu.RegisterFile.Low);
            Assert.AreEqual(highLow.Value.High, cpu.RegisterFile.High);
        }

        var expectedMemory = @case.ExpectedMemory;
        if (expectedMemory is not null)
        {
            var buffer = new byte[expectedMemory.Value.Data.Length];
            computer.Memory.Read(ulong.CreateTruncating(expectedMemory.Value.Address), buffer);
            CollectionAssert.AreEqual(expectedMemory.Value.Data, buffer);
        }

        var expectedFloatWord = @case.ExpectedWordFloatWriteBack;
        if (expectedFloatWord.HasValue)
        {
            Assert.AreEqual(expectedFloatWord.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatWord.Value.Value, execution.FWordWriteBack);
            Assert.AreEqual(expectedFloatWord.Value.Value, computer.Cpu.FloatProcessor.Words[execution.FloatReg]);
        }

        var expectedFloatLong = @case.ExpectedLongFloatWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.AreEqual(expectedFloatLong.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatLong.Value.Value, execution.FLongWriteBack);
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Cpu.FloatProcessor.Longs[execution.FloatReg]);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            if (!cpu.Config.DisableDelaySlots && execution.SideEffect is SideEffect.ProgramCounter)
            {
                // Assert the branch has not occured, then execute a NOP to apply the delayed branch
                Assert.AreEqual((uint)4, computer.Cpu.ProgramCounter);
                computer.Cpu.Insert(MipsInstruction.NOP, out _);
            }

            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }

    private static void RunJitChecks<T>(MipsComputer computer, MipsInstruction instruction, ExecutionTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var cpu = (MipsJitCpu<T>)computer.Cpu;
        cpu.Insert(instruction, out var trap);

        // Ensure that the expected trap was raised (if any)
        Assert.AreEqual(@case.ExpectedTrap, trap);

        var writeback = @case.ExpectedWriteBack;
        if (writeback.HasValue)
        {
            var writeBackValue = writeback.Value.Value;
            if (writeBackValue.HasValue)
            {
                Assert.AreEqual(writeBackValue.Value, cpu[writeback.Value.Register]);
            }
        }

        var highLow = @case.ExpectedHighLow;
        if (highLow.HasValue)
        {
            Assert.AreEqual(highLow.Value.Low, cpu.RegisterFile.Low);
            Assert.AreEqual(highLow.Value.High, cpu.RegisterFile.High);
        }

        var expectedMemory = @case.ExpectedMemory;
        if (expectedMemory is not null)
        {
            var buffer = new byte[expectedMemory.Value.Data.Length];
            computer.Memory.Read(ulong.CreateTruncating(expectedMemory.Value.Address), buffer);
            CollectionAssert.AreEqual(expectedMemory.Value.Data, buffer);
        }

        var expectedFloatWord = @case.ExpectedWordFloatWriteBack;
        if (expectedFloatWord.HasValue)
        {
            Assert.AreEqual(expectedFloatWord.Value.Value, computer.Cpu.FloatProcessor.Words[expectedFloatWord.Value.Register]);
        }

        var expectedFloatLong = @case.ExpectedLongFloatWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Cpu.FloatProcessor.Longs[expectedFloatLong.Value.Register]);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }
}
