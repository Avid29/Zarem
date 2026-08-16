// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Models.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private void BitCountSignExtend<TData>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, IBinaryInteger<TData>, IUnsignedNumber<TData>
    {
        Action<ILGenerator, RiscVInstruction, T, bool> func = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => BitCount<TData>,
            FunctRS2Code.CountTrailingZeros => BitCount<TData>,
            FunctRS2Code.PopulationCount => BitCount<TData>,
            FunctRS2Code.SignExtendByte => SignExtend<sbyte>,
            FunctRS2Code.SignExtendHalfword => SignExtend<short>,
            _ => (il, inst, _, _) => MethodBinary<TData, int>(il, inst, nameof(T.RotateLeft)),
        };

        func(il, inst, pc, compressed);
    }

    private void BitCount<TData>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, INumber<TData>
    {
        string methodName = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => nameof(IBinaryInteger<>.LeadingZeroCount),
            FunctRS2Code.CountTrailingZeros => nameof(IBinaryInteger<>.TrailingZeroCount),
            FunctRS2Code.PopulationCount => nameof(IBinaryInteger<>.PopCount),
            _ => throw new InvalidOperationException($"Unsupported bit count operation: {inst.RSCode}"),
        };

        MethodUnary<TData>(il, inst, methodName);
    }

    private void ShiftAdd<TData>(ILGenerator il, RiscVInstruction inst, int shiftAmount)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            // Load and shift rs1
            EmitLoadRegister<TData>(il, inst.RS1);
            il.EmitLoadConstant(shiftAmount);
            il.Emit(OpCodes.Shl);

            // Load and add rs2
            EmitLoadRegister<TData>(il, inst.RS2);
            il.Emit(OpCodes.Add);
        });

        // Convert to T if neccesary
        if (sizeof(TData) != sizeof(T))
            il.EmitConv<T>(IsSigned<TData>());
    }

    private void SignExtend<TData>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, IBinaryInteger<TData>
        => MethodUnary<T>(il, inst, il => il.EmitConv<TData>(Sign.Signed));

    private void BitModifiedAluR<TData>(ILGenerator il, RiscVInstruction inst, OpCode ilOpCode)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            EmitLoadRegister<TData>(il, inst.RS1);
            EmitLoadRegister<TData>(il, inst.RS2);
            if (inst.Funct7 is Funct7Code.Modified)
            {
                il.Emit(OpCodes.Not);
            }

            il.Emit(ilOpCode);
        });

        // Convert to T if neccesary
        if (sizeof(TData) != sizeof(T))
            il.EmitConv<T>(IsSigned<TData>());
    }
}
