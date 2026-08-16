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

public partial class RiscVJitCompiler<T, TFloat>
{
    private void BitCountSignExtend(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
    {
        Action<ILGenerator, RiscVInstruction, T, bool> func = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => BitCount,
            FunctRS2Code.CountTrailingZeros => BitCount,
            FunctRS2Code.PopulationCount => BitCount,
            FunctRS2Code.SignExtendByte => SignExtend<sbyte>,
            FunctRS2Code.SignExtendHalfword => SignExtend<short>,
            _ => IllegalInstruction,
        };

        func(il, inst, pc, compressed);
    }

    private void BitCount(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
    {
        string methodName = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => nameof(IBinaryInteger<>.LeadingZeroCount),
            FunctRS2Code.CountTrailingZeros => nameof(IBinaryInteger<>.TrailingZeroCount),
            FunctRS2Code.PopulationCount => nameof(IBinaryInteger<>.PopCount),
            _ => throw new InvalidOperationException($"Unsupported bit count operation: {inst.RSCode}"),
        };

        MethodInfo method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, [typeof(T)])!;
        MethodUnary<T>(il, inst, il => il.Emit(OpCodes.Call, method));
    }

    private void SignExtend<TFormat>(ILGenerator il, RiscVInstruction inst, T pc, bool compressed)
        where TFormat : unmanaged, IBinaryInteger<TFormat>
        => MethodUnary<T>(il, inst, il => il.EmitConv<TFormat>(Sign.Signed));
}
