// Avishai Dernis 2026

using System.Reflection.Emit;
using Zarem.Emulator.JIT;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public partial class RiscVJitCompiler<T, TFloat>
{
    private const int MaxBlockSize = 1024;

    /// <inheritdoc/>
    protected override JitBlockContext<T, RiscVGpRegister, RiscVTrap> DiscoverBlock(ILGenerator il, T startPc)
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

        return new(il, startPc, currentPc);
    }

    /// <inheritdoc/>
    protected override void ScanRegisterUsage(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context)
    {
        var file = Cpu.RegisterFile;
        T increment;
        for (T pc = context.Start; pc <= context.End; pc += increment)
        {
            var inst = Fetch(pc, out var decompressed);
            LogRegisterUsage(context, decompressed);
            increment = T.CreateTruncating(inst.IsCompressed ? 2 : 4);
        }

        context.UnregisterLoadUsage(file, RiscVGpRegister.Zero);
        context.UnregisterStoreUsage(file, RiscVGpRegister.Zero);
    }

    private void LogRegisterUsage(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
    {
        var rs1 = inst.RS1;
        var rs2 = inst.RS2;
        var rd = inst.RD;

        context.RegisterLoadUsage(Cpu.RegisterFile, rs1);
        context.RegisterLoadUsage(Cpu.RegisterFile, rs2);
        context.RegisterStoreUsage(Cpu.RegisterFile, rd);
    }

    private RiscVInstruction Fetch(T pc, out RiscVInstruction decompressed)
    {
        var inst = (RiscVInstruction)Cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));
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
