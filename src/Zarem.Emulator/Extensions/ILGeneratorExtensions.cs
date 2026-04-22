// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="ILGenerator"/>.
/// </summary>
public unsafe static class ILGeneratorExtensions
{
    extension(ILGenerator il)
    {
        /// <summary>
        /// Emits a load indirect op-code for the <typeparamref name="TData"/>.
        /// </summary>
        public void EmitLdind<TData>()
        {
            if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Ldind_I4);
            else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Ldind_I4);
            else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Ldind_R4);
            else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Ldind_I8);
            else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Ldind_I8);
            else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Ldind_R8);
            else throw new NotSupportedException("Unsupported register width.");
        }

        /// <summary>
        /// Emits a store indirect op-code for the <typeparamref name="TData"/>.
        /// </summary>
        public void EmitStind<TData>()
            where TData : unmanaged
        {
            if (typeof(TData) == typeof(int)) il.Emit(OpCodes.Stind_I4);
            else if (typeof(TData) == typeof(uint)) il.Emit(OpCodes.Stind_I4);
            else if (typeof(TData) == typeof(float)) il.Emit(OpCodes.Stind_R4);
            else if (typeof(TData) == typeof(long)) il.Emit(OpCodes.Stind_I8);
            else if (typeof(TData) == typeof(ulong)) il.Emit(OpCodes.Stind_I8);
            else if (typeof(TData) == typeof(double)) il.Emit(OpCodes.Stind_R8);
            else throw new NotSupportedException("Unsupported register width.");
        }

        /// <summary>
        /// Emits a convert op-code for converting to <typeparamref name="TData"/>.
        /// </summary>
        public void EmitConv<TData>(Sign sign = Sign.Unspecified)
            where TData : unmanaged
        {
            Type targetType;
            int size = sizeof(TData);
            if (sign is Sign.Unspecified) targetType = typeof(TData);
            else if (size == 1) targetType = sign == Sign.Signed ? typeof(sbyte) : typeof(byte);
            else if (size == 2) targetType = sign == Sign.Signed ? typeof(short) : typeof(ushort);
            else if (size == 4) targetType = sign == Sign.Signed ? typeof(int) : typeof(uint);
            else if (size == 8) targetType = sign == Sign.Signed ? typeof(long) : typeof(ulong);
            else throw new InvalidOperationException();

            if (targetType == typeof(sbyte)) il.Emit(OpCodes.Conv_I1);
            else if (targetType == typeof(byte)) il.Emit(OpCodes.Conv_U1);
            else if (targetType == typeof(short)) il.Emit(OpCodes.Conv_I2);
            else if (targetType == typeof(ushort)) il.Emit(OpCodes.Conv_U2);
            else if (targetType == typeof(int)) il.Emit(OpCodes.Conv_I4);
            else if (targetType == typeof(uint)) il.Emit(OpCodes.Conv_U4);
            else if (targetType == typeof(float)) il.Emit(OpCodes.Conv_R4);
            else if (targetType == typeof(long)) il.Emit(OpCodes.Conv_I8);
            else if (targetType == typeof(ulong)) il.Emit(OpCodes.Conv_U8);
            else if (targetType == typeof(double)) il.Emit(OpCodes.Conv_R8);
        }

        /// <summary>
        /// Emits a CIL instruction to load a constant value.
        /// </summary>
        public void EmitLoadConstant<TData>(TData value)
            where TData : unmanaged, INumber<TData>
        {
            if (typeof(TData) == typeof(int) || typeof(TData) == typeof(uint))
            {
                var iValue = int.CreateTruncating(value);
                var opCode = iValue switch
                {
                    -1 => OpCodes.Ldc_I4_M1,
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                    >= sbyte.MinValue and <= sbyte.MaxValue => OpCodes.Ldc_I4_S,
                    _ => OpCodes.Ldc_I4,
                };

                if (opCode == OpCodes.Ldc_I4) il.Emit(opCode, iValue);
                else if (opCode == OpCodes.Ldc_I4_S) il.Emit(opCode, (sbyte)iValue);
                else il.Emit(opCode);
            }
            else if (typeof(TData) == typeof(long) || typeof(TData) == typeof(ulong))
            {
                long lValue = long.CreateTruncating(value);

                // Optimization: If the 64-bit constant fits in a 32-bit integer, load the integer and convert.
                // The theory here is that this allows what would be a 9 byte instruction to become either a 2-6 byte
                // instruction, resulting in a smaller CIL JIT for a change that is optimized away by the CLR. Discuss.
                if (lValue >= int.MinValue && lValue <= int.MaxValue)
                {
                    EmitLoadConstant(il, (int)lValue);
                    il.Emit(OpCodes.Conv_I8);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, lValue);
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported register width.");
            }
        }

    }
}
