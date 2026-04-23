// Avishai Dernis 2026

using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

public partial class RiscVJitCompiler<T, TSigned>
{
    private const int MaxBlockSize = 1024;

    private T DiscoverBlock(T startPc)
    {
        T currentPc = startPc;

        while (true)
        {
            var inst = Fetch(currentPc);

            if (IsControlFlow(inst) || currentPc - startPc > T.CreateTruncating(MaxBlockSize))
            {
                currentPc += T.CreateTruncating(4);
                break;
            }

            currentPc += T.CreateTruncating(4);
        }

        return currentPc;
    }

    private void ScanRegisterUsage(T start, T end)
    {
        _loadRegs.Clear();
        _storeRegs.Clear();

        // TODO: Explicitly handle delay slot
        for (T pc = start; pc <= end; pc += T.CreateTruncating(4))
            LogRegisterUsage(Fetch(pc));

        _loadRegs.Remove(RiscVGpRegister.Zero);
        _storeRegs.Remove(RiscVGpRegister.Zero);
    }

    private void LogRegisterUsage(RiscVInstruction inst)
    {
        var rs1 = inst.RS1;
        var rs2 = inst.RS2;
        var rd = inst.RD;

        _loadRegs.Add(rs1);
        _loadRegs.Add(rs2);
        _storeRegs.Add(rd);
    }

    private RiscVInstruction Fetch(T pc) => (RiscVInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));

    private static bool IsControlFlow(RiscVInstruction inst)
    {
        return inst.OpCode switch
        {
            RiscVOpCode.JumpAndLink or RiscVOpCode.JumpAndLinkRegister or
            RiscVOpCode.Branch => true,
            _ => false,
        };
    }
}
