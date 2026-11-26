// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Emulator.JIT;

public unsafe partial class RiscVJitCompiler<T, TFloat>
{
    private void BitCountSignExtend<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, IBinaryInteger<TData>, IUnsignedNumber<TData>
    {
        Action<JitBlockContext<T, RiscVGpRegister, RiscVTrap>, RiscVInstruction, T, bool> func = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => BitCount<TData>,
            FunctRS2Code.CountTrailingZeros => BitCount<TData>,
            FunctRS2Code.PopulationCount => BitCount<TData>,
            FunctRS2Code.SignExtendByte => SignExtend<sbyte>,
            FunctRS2Code.SignExtendHalfword => SignExtend<short>,
            _ => (il, inst, _, _) => MethodBinary<TData, int>(context, inst, nameof(T.RotateLeft)),
        };

        func(context, inst, pc, compressed);
    }

    private void BitCount<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, INumber<TData>
    {
        string methodName = inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => nameof(IBinaryInteger<>.LeadingZeroCount),
            FunctRS2Code.CountTrailingZeros => nameof(IBinaryInteger<>.TrailingZeroCount),
            FunctRS2Code.PopulationCount => nameof(IBinaryInteger<>.PopCount),
            _ => throw new InvalidOperationException($"Unsupported bit count operation: {inst.RSCode}"),
        };

        MethodUnary<TData>(context, inst, methodName);
    }

    private void ShiftAdd<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, int shiftAmount)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load and shift rs1
            EmitLoadRegister<TData>(context, inst.RS1);
            il.EmitLoadConstant(shiftAmount);
            il.Emit(OpCodes.Shl);

            // Load and add rs2
            EmitLoadRegister<TData>(context, inst.RS2);
            il.Emit(OpCodes.Add);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
    }

    private void SignExtend<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, T pc, bool compressed)
        where TData : unmanaged, IBinaryInteger<TData>
        => MethodUnary<T>(context, inst, il => il.EmitConv<TData>(Sign.Signed));

    private void BitSetClearR<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, bool set)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        BitManipulateR<TData>(context, inst, (il) =>
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

    private void BitSetClearI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, bool set)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        BitManipulateI<TData>(context, inst, il =>
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

    private void BitInvertR<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, IBinaryInteger<TData> => BitManipulateR<TData>(context, inst, (il) => il.Emit(OpCodes.Xor));

    private void BitInvertI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, IBinaryInteger<TData> => BitManipulateI<TData>(context, inst, (il) => il.Emit(OpCodes.Xor));

    private void BitManipulateR<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, Action<ILGenerator> action)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load rs1
            EmitLoadRegister<TData>(context, inst.RS1);

            // Load rs2 and create mask
            il.EmitLoadConstant(TData.One);
            EmitLoadRegister<int>(context, inst.RS2);
            il.Emit(OpCodes.Shl);

            action(il);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
    }

    private void BitManipulateI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst, Action<ILGenerator> action)
        where TData : unmanaged, IBinaryInteger<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load rs1
            EmitLoadRegister<TData>(context, inst.RS1);

            // Load mask
            il.EmitLoadConstant(TData.One << inst.ShiftImmediate);

            action(il);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
    }

    private void BitExtractR<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load rs1 as TData and rs2 as int
            EmitLoadRegister<TData>(context, inst.RS1);
            EmitLoadRegister<int>(context, inst.RS2);

            // Shift rs1 by rs2 and mask bit 1
            il.Emit(OpCodes.Shr);
            il.EmitLoadConstant(TData.One);
            il.Emit(OpCodes.And);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
    }

    private void BitExtractI<TData>(JitBlockContext<T, RiscVGpRegister, RiscVTrap> context, RiscVInstruction inst)
        where TData : unmanaged, INumber<TData>
    {
        EmitStoreRegister(context, inst.RD, il =>
        {
            // Load rs1 as TData and rs2 as int
            EmitLoadRegister<TData>(context, inst.RS1);
            il.EmitLoadConstant<int>(inst.ShiftImmediate);

            // Shift rs1 by rs2 and mask bit 1
            il.Emit(OpCodes.Shr);
            il.EmitLoadConstant(TData.One);
            il.Emit(OpCodes.And);
        });

        // Convert to T if neccesary
        context.ILGenerator.EmitConv<TData, T>(IsSigned<TData>());
    }
}
