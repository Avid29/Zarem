// Avishai Dernis 2026

using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public partial class RiscVJitCompiler<T, TFloat>
{
    private const int MaxBlockSize = 1024;

    private T DiscoverBlock(T startPc)
    {
        T currentPc = startPc;

        while (true)
        {
            var inst = Fetch(currentPc, out var decompressed);
            var increment = T.CreateTruncating(inst.IsCompressed ? 2 : 4);

            if (IsControlFlow(decompressed) || currentPc - startPc > T.CreateTruncating(MaxBlockSize))
            {
                currentPc += increment;
                break;
            }

            currentPc += increment;
        }

        return currentPc;
    }

    private void ScanRegisterUsage(T start, T end)
    {
        _loadRegs.Clear();
        _storeRegs.Clear();

        T increment;
        for (T pc = start; pc <= end; pc += increment)
        {
            var inst = Fetch(pc, out var decompressed);
            LogRegisterUsage(decompressed);
            increment = T.CreateTruncating(inst.IsCompressed ? 2 : 4);
        }

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

    private RiscVInstruction Fetch(T pc, out RiscVInstruction decompressed)
    {
        var inst = (RiscVInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));
        decompressed = inst;

        if (inst.IsCompressed)
        {
            _decompressor?.Decompress((RiscVCompressedInstruction)inst, out decompressed);
        }

        return inst;
    }

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
