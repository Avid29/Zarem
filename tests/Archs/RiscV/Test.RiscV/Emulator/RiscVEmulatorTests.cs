// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Assembler;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator.Config.Enums;
using Zarem.RiscV.Assembler;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.JIT;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Test.RiscV.Emulator;

[TestClass]
public partial class RiscVEmulatorTests
{
    public const uint S8 = 0xbd0;
    public const uint S9 = 0x516;

    [DataTestMethod("RV32I")]
    [RiscVEmulatorTestDataSource("RV32I", ExecutionMode.Interpret)]
    public void InstructionTests_RV32I(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, byte>(@case);

    [DataTestMethod("RV32I (JIT)")]
    [RiscVEmulatorTestDataSource("RV32I", ExecutionMode.JustInTime)]
    public void InstructionTests_RV32I_JIT(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, byte>(@case);

    [DataTestMethod("RV32G")]
    [RiscVEmulatorTestDataSource("RV32G", ExecutionMode.Interpret)]
    public void InstructionTests_RV32G(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, ulong>(@case);

    [DataTestMethod("RV32G (JIT)")]
    [RiscVEmulatorTestDataSource("RV32G", ExecutionMode.JustInTime)]
    public void InstructionTests_RV32G_JIT(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, ulong>(@case);

    [DataTestMethod("RV32GCB")]
    [RiscVEmulatorTestDataSource("RV32GCB", ExecutionMode.Interpret)]
    public void InstructionTests_RV32GCB(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, ulong>(@case);

    [DataTestMethod("RV32GCB (JIT)")]
    [RiscVEmulatorTestDataSource("RV32GCB", ExecutionMode.JustInTime)]
    public void InstructionTests_RV32GCB_JIT(RiscVEmulatorTestCase<uint> @case) => RunTest<uint, ulong>(@case);

    [DataTestMethod("RV64I")]
    [RiscVEmulatorTestDataSource("RV64I", ExecutionMode.Interpret)]
    public void InstructionTests_RV64I(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, byte>(@case);

    [DataTestMethod("RV64I (JIT)")]
    [RiscVEmulatorTestDataSource("RV64I", ExecutionMode.JustInTime)]
    public void InstructionTests_RV64I_JIT(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, byte>(@case);

    [DataTestMethod("RV64G")]
    [RiscVEmulatorTestDataSource("RV64G", ExecutionMode.Interpret)]
    public void InstructionTests_RV64G(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, ulong>(@case);

    [DataTestMethod("RV64G (JIT)")]
    [RiscVEmulatorTestDataSource("RV64G", ExecutionMode.JustInTime)]
    public void InstructionTests_RV64G_JIT(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, ulong>(@case);

    [DataTestMethod("RV64GCB")]
    [RiscVEmulatorTestDataSource("RV64GCB", ExecutionMode.Interpret)]
    public void InstructionTests_RV64GCB(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, ulong>(@case);

    [DataTestMethod("RV64GCB (JIT)")]
    [RiscVEmulatorTestDataSource("RV64GCB", ExecutionMode.JustInTime)]
    public void InstructionTests_RV64GCB_JIT(RiscVEmulatorTestCase<ulong> @case) => RunTest<ulong, ulong>(@case);

    [DataTestMethod("RV128I")]
    [RiscVEmulatorTestDataSource("RV128I", ExecutionMode.Interpret)]
    public void InstructionTests_RV128I(RiscVEmulatorTestCase<UInt128> @case) => RunTest<UInt128, byte>(@case);

    [DataTestMethod("RV128G")]
    [RiscVEmulatorTestDataSource("RV128G", ExecutionMode.Interpret)]
    public void InstructionTests_RV128G(RiscVEmulatorTestCase<UInt128> @case) => RunTest<UInt128, byte>(@case);

    private static void RunTest<T, TFloat>(RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var config = @case.Config;

        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input, RiscVTokenizerProfile.Default)[0];
        var table = new RiscVInstructionTable(new(config.VersionInfo));
        var parser = new RiscVInstructionParser(new(config.VersionInfo), table, default, null, null);
        var parsed = parser.Parse(tokenized, out _);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed[0];
        var computer = new RiscVComputer(config);
        var cpu = (RiscVCpu<T>)computer.Cpu;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            cpu[reg] = value;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.FPRInitialization)
            cpu.FloatRegisterFile?.Singles[(int)reg] = value;

        // Initialize the memory, if specified in the test case
        foreach (var (address, data) in @case.MemoryInitialization)
            computer.Memory.Write(ulong.CreateTruncating(address), data);

        if (cpu is RiscVJitCpu<T, TFloat>)
        {
            RunJitChecks<T, TFloat>(computer, instruction, @case);
        }
        else if (cpu is RiscVInterpretCpu<T, TFloat>)
        {
            RunInterpretChecks<T, TFloat>(computer, instruction, @case);
        }
    }

    private static void RunInterpretChecks<T, TFloat>(RiscVComputer computer, RiscVInstruction instruction, RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var cpu = (RiscVInterpretCpu<T, TFloat>)computer.Cpu;
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

        var expectedSingle = @case.ExpectedSingleWriteBack;
        if (expectedSingle.HasValue)
        {
            Assert.IsNotNull(computer.Cpu.FloatRegisterFile);
            Assert.AreEqual(expectedSingle.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedSingle.Value.Value, execution.SingleWriteBack);
            Assert.AreEqual(expectedSingle.Value.Value, computer.Cpu.FloatRegisterFile.Singles[(int)execution.FloatReg]);
        }

        var expectedFloatLong = @case.ExpectedDoubleWriteBack;
        if (expectedFloatLong.HasValue)
        {
            Assert.IsNotNull(computer.Cpu.FloatRegisterFile);
            Assert.AreEqual(expectedFloatLong.Value.Register, execution.FloatReg);
            Assert.AreEqual(expectedFloatLong.Value.Value, execution.DoubleWriteBack);
            Assert.AreEqual(expectedFloatLong.Value.Value, computer.Cpu.FloatRegisterFile.Doubles[(int)execution.FloatReg]);
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

    private static void RunJitChecks<T, TFloat>(RiscVComputer computer, RiscVInstruction instruction, RiscVEmulatorTestCase<T> @case)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>, IMinMaxValue<TFloat>
    {
        var cpu = (RiscVJitCpu<T, TFloat>)computer.Cpu;
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
