// Avishai Dernis 2026

using System.Reflection.Emit;
using Zarem.Emulator.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Models.JIT;

public partial class MipsJitCompiler<T>
{
    private const int MaxBlockSize = 1024;

    /// <inheritdoc/>
    protected override JitBlockContext<T, MipsGpRegister, MipsTrap> DiscoverBlock(ILGenerator il, T startPc)
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

        return new(il, startPc, currentPc);
    }

    /// <inheritdoc/>
    protected override void ScanRegisterUsage(JitBlockContext<T, MipsGpRegister, MipsTrap> context)
    {
        var file = Cpu.RegisterFile;
        context.RegisterLoadUsage(file, MipsGpRegister.Low);
        context.RegisterLoadUsage(file, MipsGpRegister.High);
        context.RegisterStoreUsage(file, MipsGpRegister.Low);
        context.RegisterStoreUsage(file, MipsGpRegister.High);
        context.RegisterStoreUsage(file, MipsGpRegister.ReturnAddress);

        // TODO: Explicitly handle delay slot
        for (T pc = context.Start; pc <= context.End; pc += T.CreateTruncating(4))
            LogRegisterUsage(context, Fetch(pc));

        context.UnregisterLoadUsage(file, MipsGpRegister.Zero);
        context.UnregisterStoreUsage(file, MipsGpRegister.Zero);
    }

    private void LogRegisterUsage(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction inst)
    {
        var rs = inst.RS;
        var rt = inst.RT;
        var rd = inst.RD;

        context.RegisterLoadUsage(Cpu.RegisterFile, rs);
        context.RegisterLoadUsage(Cpu.RegisterFile, rt);
        context.RegisterLoadUsage(Cpu.RegisterFile, rd);
        context.RegisterStoreUsage(Cpu.RegisterFile, rd);
        context.RegisterStoreUsage(Cpu.RegisterFile, rt);
    }

    private MipsInstruction Fetch(T pc) => (MipsInstruction)Cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));

    private static bool IsControlFlow(MipsInstruction inst)
    {
        return inst.OpCode switch
        {
            MipsOpCode.Special => inst.FuncCode switch
            {
                FunctionCode.JumpRegister => true,
                FunctionCode.JumpAndLinkRegister => true,
                >= FunctionCode.TrapOnGreaterOrEqual and <= FunctionCode.TrapOnNotEquals => true,
                _ => false
            },

            >= MipsOpCode.RegisterImmediate and <= MipsOpCode.BranchOnGreaterThanZero => true,
            >= MipsOpCode.BranchOnEqualLikely and <= MipsOpCode.BranchOnGreaterThanZeroLikely => true,
            _ => false,
        };
    }
}
