// Avishai Dernis 2024

using Test.Mips.Helpers;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Test.Mips;

[TestClass]
public class EncodingTests
{
    [TestMethod("Set OpCode")]
    public void SetOpCodeTest()
    {
        // This test sets the opcode to each potential operation code with a random address.
        // It then asserts the readback is equivilient.
        for (var i = MipsOpCode.Special; i <= MipsOpCode.StoreWordCoprocessor3; i++)
        {
            var instruction = MipsInstruction.CreateJ(i, ArgGenerator.RandomAddress(false));
            Assert.AreEqual(i, instruction.OpCode, $"Error setting operation code to {i}");
        }
    }

    [TestMethod("Set Registers")]
    public void SetRegistersTest()
    {
        // This test sets each register argument to each potential register with an otherwise random instruction.
        // It then asserts the readback is equivilient.
        for (var i = MipsGpRegister.Zero; i <= MipsGpRegister.ReturnAddress; i++)
        {
            var instruction = MipsInstruction.CreateI(
                ArgGenerator.RandomOpCode(false),
                i,
                ArgGenerator.RandomRegister(false),
                ArgGenerator.RandomImmediate(false));
            Assert.AreEqual(i, instruction.RS, $"Error setting rs register to {i}");

            instruction = MipsInstruction.CreateI(
                ArgGenerator.RandomOpCode(false),
                ArgGenerator.RandomRegister(false),
                i,
                ArgGenerator.RandomImmediate(false));
            Assert.AreEqual(i, instruction.RT, $"Error setting rt register to {i}");

            instruction = MipsInstruction.CreateR(
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
            var instruction = MipsInstruction.CreateJ(ArgGenerator.RandomOpCode(false), addr);
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
            var instruction = MipsInstruction.CreateBranch(RegImmFuncCode.BranchOnLessThanZero, ArgGenerator.RandomRegister(), offset);
            Assert.AreEqual(offset, instruction.Offset, $"Error setting offset to {offset}");
        }
    }
}
