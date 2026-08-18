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

    private void BitSetClearR<TData>(ILGenerator il, RiscVInstruction inst, bool set)
        where TData : unmanaged, INumber<TData>
    {
        BitManipulateR<TData>(il, inst, (il) =>
        {
            if (set)
            {
                // Set
                il.Emit(OpCodes.Or);
            }
            else
            {
                // Clear
                il.Emit(OpCodes.Not);
                il.Emit(OpCodes.And);
            }
        });
    }

    private void BitInvertR<TData>(ILGenerator il, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData> => BitManipulateR<TData>(il, inst, (il) => il.Emit(OpCodes.Xor));

    private void BitManipulateR<TData>(ILGenerator il, RiscVInstruction inst, Action<ILGenerator> action)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            // Load rs1
            EmitLoadRegister<TData>(il, inst.RS1);

            // Load rs2 and create mask
            il.EmitLoadConstant(TData.One);
            EmitLoadRegister<int>(il, inst.RS2);
            il.Emit(OpCodes.Shl);

            action(il);
        });

        // Convert to T if neccesary
        if (sizeof(TData) != sizeof(T))
            il.EmitConv<T>(IsSigned<TData>());
    }

    private void BitExtractR<TData>(ILGenerator il, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(il, inst.RD, il =>
        {
            // Load rs1 as TData and rs2 as int
            EmitLoadRegister<TData>(il, inst.RS1);
            EmitLoadRegister<int>(il, inst.RS2);

            // Shift rs1 by rs2 and mask bit 1
            il.Emit(OpCodes.Shr);
            il.EmitLoadConstant(TData.One);
            il.Emit(OpCodes.And);
        });

        // Convert to T if neccesary
        if (sizeof(TData) != sizeof(T))
            il.EmitConv<T>(IsSigned<TData>());
    }
}
