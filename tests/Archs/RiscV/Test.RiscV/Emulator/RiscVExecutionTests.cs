// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Assembler;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Test.RiscV.Emulator;

[TestClass]
public partial class RiscVExecutionTests
{
    public const uint K0 = 0xbd0;
    public const uint K1 = 0x516;

    [DataTestMethod]
    [RiscVInstructionSource("RV32I", ExecutionMode.Interpret)]
    public void InstructionTests_RV32_I(RiscVEmulatorTestCase<uint> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV32I", ExecutionMode.JustInTime)]
    public void InstructionTests_RV32_I_JIT(RiscVEmulatorTestCase<uint> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV32G", ExecutionMode.Interpret)]
    public void InstructionTests_RV32_G(RiscVEmulatorTestCase<uint> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV32G", ExecutionMode.JustInTime)]
    public void InstructionTests_RV32_G_JIT(RiscVEmulatorTestCase<uint> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV64I", ExecutionMode.Interpret)]
    public void InstructionTests_RV64_I(RiscVEmulatorTestCase<ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV64G", ExecutionMode.Interpret)]
    public void InstructionTests_RV64_G(RiscVEmulatorTestCase<ulong> @case) => RunTest(@case);

    [DataTestMethod]
    [RiscVInstructionSource("RV128I", ExecutionMode.Interpret)]
    public void InstructionTests_RV128_I(RiscVEmulatorTestCase<UInt128> @case) => RunTest(@case);

    private static void RunTest<T>(RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var config = @case.Config;

        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input, RiscVTokenizerProfile.Default)[0];
        var table = new RiscVInstructionTable(new(config.VersionInfo));
        var parser = new RiscVInstructionParser(new(), table, default, null, null);
        var parsed = parser.Parse(tokenized);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed.Instructions[0];
        var computer = new RiscVComputer(config);
        var cpu = (RiscVCpu<T>)computer.Cpu;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            cpu[reg] = value;

        // Initialize the memory, if specified in the test case
        foreach (var (address, data) in @case.MemoryInitialization)
            computer.Memory.Write(ulong.CreateTruncating(address), data);

        if (cpu is RiscVJitCpu<T>)
        {
            RunJitChecks(computer, instruction, @case);
        }
        else if (cpu is RiscVInterpretCpu<T>)
        {
            RunInterpretChecks(computer, instruction, @case);
        }
    }

    private static void RunInterpretChecks<T>(RiscVComputer computer, RiscVInstruction instruction, RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var cpu = (RiscVInterpretCpu<T>)computer.Cpu;
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
            Assert.AreEqual(RiscVGpRegister.Zero, execution.WritebackGPRegister);
        }

        var expectedMemory = @case.ExpectedMemory;
        if (expectedMemory is not null)
        {
            var buffer = new byte[expectedMemory.Value.Data.Length];
            computer.Memory.Read(ulong.CreateTruncating(expectedMemory.Value.Address), buffer);
            CollectionAssert.AreEqual(expectedMemory.Value.Data, buffer);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }

    private static void RunJitChecks<T>(RiscVComputer computer, RiscVInstruction instruction, RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        var cpu = (RiscVJitCpu<T>)computer.Cpu;
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

        var expectedMemory = @case.ExpectedMemory;
        if (expectedMemory is not null)
        {
            var buffer = new byte[expectedMemory.Value.Data.Length];
            computer.Memory.Read(ulong.CreateTruncating(expectedMemory.Value.Address), buffer);
            CollectionAssert.AreEqual(expectedMemory.Value.Data, buffer);
        }

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }
}
