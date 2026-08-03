// Avishai Dernis 2026

using System.Numerics;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator.Config.Enums;
using Zarem.Mips.Assembler;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.JIT;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Test.Mips.Emulator;

[TestClass]
public partial class MipsExecutionTests
{
    public const uint K0 = 0xbd0;
    public const uint K1 = 0xd16;

    [DataTestMethod]
    [MipsInstructionSource("mips1", ExecutionMode.Interpret)]
    public void InstructionTests_Mips1(MipsEmulatorTestCase<uint, uint> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips1", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips1_JIT(MipsEmulatorTestCase<uint, uint> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips2", ExecutionMode.Interpret)]
    public void InstructionTests_Mips2(MipsEmulatorTestCase<uint, uint> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips2", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips2_JIT(MipsEmulatorTestCase<uint, uint> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips3", ExecutionMode.Interpret)]
    public void InstructionTests_Mips3(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips3", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips3_JIT(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips3_32bit", ExecutionMode.Interpret)]
    public void InstructionTests_Mips3_32Bit(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips3_32bit", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips3_32Bit_JIT(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips4", ExecutionMode.Interpret)]
    public void InstructionTests_Mips4(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips4", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips4_JIT(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips4_32bit", ExecutionMode.Interpret)]
    public void InstructionTests_Mips4_32Bit(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips4_32bit", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips4_32Bit_JIT(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips5", ExecutionMode.Interpret)]
    public void InstructionTests_Mips5(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips5", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips5_JIT(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips5_32bit", ExecutionMode.Interpret)]
    public void InstructionTests_Mips5_32Bit(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips5_32bit", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips5_32Bit_JIT(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips32r1", ExecutionMode.Interpret)]
    public void InstructionTests_Mips32R1(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips32r1", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips32R1_JIT(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips64r1", ExecutionMode.Interpret)]
    public void InstructionTests_Mips64R1(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips64r1", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips64R1_JIT(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips32r2", ExecutionMode.Interpret)]
    public void InstructionTests_Mips32R2(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips32r2", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips32R2_JIT(MipsEmulatorTestCase<uint, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips64r2", ExecutionMode.Interpret)]
    public void InstructionTests_Mips64R2(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [MipsInstructionSource("mips64r2", ExecutionMode.JustInTime)]
    public void InstructionTests_Mips64R2_JIT(MipsEmulatorTestCase<ulong, ulong> @case) => RunTest(@case);

    private static void RunTest<T, TFloat>(MipsEmulatorTestCase<T, TFloat> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var config = @case.Config;

        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input, MipsTokenizerProfile.Default)[0];
        var table = new MipsInstructionTable(new MipsAssemblerConfig(config.VersionInfo));
        var parser = new MipsInstructionParser(new(), table, default, null, null);
        var parsed = parser.Parse(tokenized, out _);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed[0];
        var computer = new MipsComputer(config);
        var cpu = (MipsCpu<T, TFloat>)computer.Cpu;

        // Initialize the status register
        cpu.CoProcessor0.RegisterFile.StatusRegister = @case.Status;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            cpu[reg] = value;

        foreach (var (reg, value) in @case.FPRInitialization)
        {
            cpu.FloatProcessor[reg] = TFloat.CreateTruncating(value);
        }

        // Initialize the high and low registers if specified in the test case
        if (@case.InitialHighLow.HasValue)
        {
            cpu.RegisterFile.Low = @case.InitialHighLow.Value.Low;
            cpu.RegisterFile.High = @case.InitialHighLow.Value.High;
        }

        // Initialize TLB
        cpu.Tlb.InitilizeSegment(0, 0, 0x1_0000);

        // Initialize the memory, if specified in the test case
        foreach (var (address, data) in @case.MemoryInitialization)
            computer.Memory.Write(ulong.CreateTruncating(address), data);

        // Initialize the program counter
        cpu.ProgramCounter = @case.InitialPC;

        if (cpu is MipsJitCpu<T, TFloat>)
        {
            RunJitChecks(computer, instruction, @case);
        }
        else if (cpu is MipsInterpretCpu<T, TFloat>)
        {
            RunInterpretChecks(computer, instruction, @case);
        }
    }

    private static void RunInterpretChecks<T, TFloat>(MipsComputer computer, MipsInstruction instruction, MipsEmulatorTestCase<T, TFloat> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var cpu = (MipsInterpretCpu<T, TFloat>)computer.Cpu;
        cpu.Insert(instruction, out var execution, out var trap);

        // Ensure that the expected trap was raised (if any)
        Assert.AreEqual(@case.ExpectedTrap, trap);

        var writeback = @case.ExpectedWriteBack;
        if (writeback.HasValue)
        {
            // Ensure that the expected register was written to with the expected value
            Assert.AreEqual(writeback.Value.Register, execution.WritebackGPRegister);

            var writeBackValue = writeback.Value.Value;
            if (writeBackValue.HasValue)
            {
                Assert.AreEqual(writeBackValue.Value, cpu[execution.WritebackGPRegister]);
            }
        }
        else
        {
            // If no register check was provided, we at least want to make sure no register was written to (as that would be unexpected)
            Assert.AreEqual(MipsGpRegister.Zero, execution.WritebackGPRegister);
        }

        var highLow = @case.ExpectedHighLow;
        if (highLow.HasValue)
        {
            Assert.AreEqual(highLow.Value, (cpu.RegisterFile.High, cpu.RegisterFile.Low));
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
            Assert.AreEqual(expectedFloatWord.Value.Value, computer.Cpu.FloatProcessor.Words[(int)execution.FloatReg]);
        }

        var expectedFloatLong = @case.ExpectedLongFloatWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.AreEqual(expectedFloatLong.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatLong.Value.Value, execution.FLongWriteBack);
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Cpu.FloatProcessor.Longs[(int)execution.FloatReg]);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            if (!cpu.Config.DisableDelaySlots && execution.SideEffect is MipsSideEffect.ProgramCounter)
            {
                // Assert the branch has not occured, then execute a NOP to apply the delayed branch
                Assert.AreEqual(ulong.CreateTruncating(@case.InitialPC) + 4, computer.Cpu.ProgramCounter);
                computer.Cpu.Insert(MipsInstruction.NOP, out _);
            }

            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }

    private static void RunJitChecks<T, TFloat>(MipsComputer computer, MipsInstruction instruction, MipsEmulatorTestCase<T, TFloat> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var cpu = (MipsJitCpu<T, TFloat>)computer.Cpu;
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
            Assert.AreEqual(highLow.Value, (cpu.RegisterFile.High, cpu.RegisterFile.Low));
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
            Assert.AreEqual(expectedFloatWord.Value.Value, computer.Cpu.FloatProcessor.Words[(int)expectedFloatWord.Value.Register]);
        }

        var expectedFloatLong = @case.ExpectedLongFloatWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Cpu.FloatProcessor.Longs[(int)expectedFloatLong.Value.Register]);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }
}
