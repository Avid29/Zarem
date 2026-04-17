// Avishai Dernis 2026

using System.Numerics;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Emulator.Models.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public partial class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
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

        _loadRegs.Add(MipsGpRegister.Low);
        _loadRegs.Add(MipsGpRegister.High);
        _storeRegs.Add(MipsGpRegister.Low);
        _storeRegs.Add(MipsGpRegister.High);
        _storeRegs.Add(MipsGpRegister.ReturnAddress);

        // TODO: Explicitly handle delay slot
        for (T pc = start; pc <= end; pc += T.CreateTruncating(4))
            LogRegisterUsage(Fetch(pc));
    }

    private void LogRegisterUsage(MipsInstruction inst)
    {
        var rs = inst.RS;
        var rt = inst.RT;
        var rd = inst.RD;

        _loadRegs.Add(rs);
        _loadRegs.Add(rt);
        _loadRegs.Add(rd);
        _storeRegs.Add(rt);
        _storeRegs.Add(rd);
    }

    private MipsInstruction Fetch(T pc) => (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(pc));

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
