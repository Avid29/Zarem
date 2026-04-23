// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Assembler;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Test.RiscV.Emulator;

[TestClass]
public partial class ExecutionTests
{
    public const uint K0 = 0xbd0;
    public const uint K1 = 0x516;

    [DataTestMethod]
    [RiscVInstructionSource("RV32I", ExecutionMode.Interpret)]
    public void InstructionTests_RV32_I(RiscVEmulatorTestCase<uint> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.Integers));

    [DataTestMethod]
    [RiscVInstructionSource("RV32G", ExecutionMode.Interpret)]
    public void InstructionTests_RV32_G(RiscVEmulatorTestCase<uint> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV32, RiscVExtensions.General));

    [DataTestMethod]
    [RiscVInstructionSource("RV64I", ExecutionMode.Interpret)]
    public void InstructionTests_RV64_I(RiscVEmulatorTestCase<ulong> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.Integers));

    [DataTestMethod]
    [RiscVInstructionSource("RV64G", ExecutionMode.Interpret)]
    public void InstructionTests_RV64_G(RiscVEmulatorTestCase<ulong> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV64, RiscVExtensions.General));

    [DataTestMethod]
    [RiscVInstructionSource("RV128I", ExecutionMode.Interpret)]
    public void InstructionTests_RV128_I(RiscVEmulatorTestCase<UInt128> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV128, RiscVExtensions.Integers));

    [DataTestMethod]
    [RiscVInstructionSource("RV128G", ExecutionMode.Interpret)]
    public void InstructionTests_RV128_G(RiscVEmulatorTestCase<UInt128> @case) => RunTest(@case, new RiscVVersionInfo(RiscVBaseVersion.RV128, RiscVExtensions.General));

    private static void RunTest<T>(RiscVEmulatorTestCase<T> @case, RiscVVersionInfo versionInfo)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        // The instruction parser is only used to convert the instruction string into an Instruction struct, so we can test the interpreter with it.
        var tokenized = Tokenizer.TokenizeLine(@case.Input, RiscVTokenizerProfile.Default)[0];
        var table = new RiscVInstructionTable(new(versionInfo));
        var parser = new RiscVInstructionParser(new(), table, default, null, null);
        var parsed = parser.Parse(tokenized);
        if (parsed is null)
            Assert.Fail();

        // TODO: Psuedo instruction support
        var instruction = parsed.Realize()[0];
        var emulatorConfig = new RiscVEmulatorConfig(versionInfo);
        var computer = new RiscVComputer(emulatorConfig);
        var cpu = (RiscVCpu<T>)computer.Cpu;

        // Initialize the register file with the provided values
        foreach (var (reg, value) in @case.RegisterInitialization)
            cpu[reg] = value;
        
        if (cpu is RiscVInterpretCpu<T>)
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
            Assert.AreEqual(writeback.Value.Regiter, execution.WritebackGPRegister);

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

        var expectedPC = @case.ExpectedPC;
        if (expectedPC is not null)
        {
            Assert.AreEqual(expectedPC.Value, cpu.ProgramCounter);
        }
    }
}
