// Avishai Dernis 2024

using Test.MIPS.Helpers;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Test.MIPS;

[TestClass]
public class InstructionTests
{
    [TestMethod("Set OpCode")]
    public void SetOpCodeTest()
    {
        // This test sets the opcode to each potential operation code with a random address.
        // It then asserts the readback is equivilient.
        for (var i = OperationCode.Special; i <= OperationCode.StoreWordCoprocessor3; i++)
        {
            var instruction = MipsInstruction.Create(i, ArgGenerator.RandomAddress(false));
            Assert.AreEqual(i, instruction.OpCode, $"Error setting operation code to {i}");
        }
    }

    [TestMethod("Set Registers")]
    public void SetRegistersTest()
    {
        // This test sets each register argument to each potential register with an otherwise random instruction.
        // It then asserts the readback is equivilient.
        for (var i = GPRegister.Zero; i <= GPRegister.ReturnAddress; i++)
        {
            var instruction = MipsInstruction.Create(
                ArgGenerator.RandomOpCode(false),
                i,
                ArgGenerator.RandomRegister(false),
                ArgGenerator.RandomImmediate(false));
            Assert.AreEqual(i, instruction.RS, $"Error setting rs register to {i}");

            instruction = MipsInstruction.Create(
                ArgGenerator.RandomOpCode(false),
                ArgGenerator.RandomRegister(false),
                i,
                ArgGenerator.RandomImmediate(false));
            Assert.AreEqual(i, instruction.RT, $"Error setting rt register to {i}");

            instruction = MipsInstruction.Create(
                ArgGenerator.RandomFuncCode(false),
                ArgGenerator.RandomRegister(false),
                ArgGenerator.RandomRegister(false),
                i);
            Assert.AreEqual(i, instruction.RD, $"Error setting rd register to {i}");
        }
    }

    [TestMethod("Set Address")]
    public void SetAddressTest()
    {
        // This test sets the address to a random value 20 times.
        // It asserts the readback is equivilient each time.
        for (var i = 0; i < 20; i++)
        {
            var addr = ArgGenerator.RandomAddress();
            var instruction = MipsInstruction.Create(ArgGenerator.RandomOpCode(false), addr);
            Assert.AreEqual(addr, instruction.Address, $"Error setting address to {addr}");
        }
    }

    [TestMethod("Set Offset")]
    public void SetOffsetTest()
    {
        // This test sets the address to a random value 20 times.
        // It asserts the readback is equivilient each time.
        for (var i = 0; i < 20; i++)
        {
            var offset = ArgGenerator.RandomOffset();
            var instruction = MipsInstruction.Create(RegImmFuncCode.BranchOnLessThanZero, ArgGenerator.RandomRegister(), offset);
            Assert.AreEqual(offset, instruction.Offset, $"Error setting offset to {offset}");
        }
    }
}
