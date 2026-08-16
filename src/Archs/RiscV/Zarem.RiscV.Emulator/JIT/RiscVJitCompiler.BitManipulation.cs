// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Models.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private void BitCountSignExtend<T2>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
    {
        Action<ILGenerator, RiscVInstruction, T, bool> func = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => BitCount<T2>,
            FunctRS2Code.CountTrailingZeros => BitCount<T2>,
            FunctRS2Code.PopulationCount => BitCount<T2>,
            FunctRS2Code.SignExtendByte => SignExtend<sbyte>,
            FunctRS2Code.SignExtendHalfword => SignExtend<short>,
            _ => (il, inst, _, _) => MethodBinary<T2, int>(il, inst, nameof(T.RotateLeft)),
        };

        func(il, inst, pc, compressed);
    }

    private void BitCount<T2>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
    {
        string methodName = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => nameof(IBinaryInteger<>.LeadingZeroCount),
            FunctRS2Code.CountTrailingZeros => nameof(IBinaryInteger<>.TrailingZeroCount),
            FunctRS2Code.PopulationCount => nameof(IBinaryInteger<>.PopCount),
            _ => throw new InvalidOperationException($"Unsupported bit count operation: {inst.RSCode}"),
        };

        MethodUnary<T>(il, inst, methodName);
    }

    private void SignExtend<TFormat>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryInteger<TFormat>
        => MethodUnary<T>(il, inst, il => il.EmitConv<TFormat>(Sign.Signed));

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
