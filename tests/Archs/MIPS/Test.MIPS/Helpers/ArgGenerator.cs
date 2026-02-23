// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Test.MIPS.Helpers;

public class ArgGenerator
{
    /// <remarks>
    /// Safe constrains the value to within the 16 bit range.
    /// For good testing purposes, this shouldn't always be used as the masking should fix overflowing immediates.
    /// </remarks>
    public static short RandomImmediate(bool safe = true) => (short)Random.Shared.Next(safe ? short.MaxValue : int.MaxValue);

    public static byte RandomShift(bool safe = true) => (byte)Random.Shared.Next(safe ? (1 << 5)-1 : int.MaxValue);

    public static int RandomOffset(bool safe = true) => Random.Shared.Next(safe ? (1 << 17)-1 : int.MaxValue) & ~0b11;

    public static uint RandomAddress(bool safe = true) => (uint)Random.Shared.Next(safe ? (1 << 26)-1 : int.MaxValue) & ~(uint)0b11;

    public static GPRegister RandomRegister(bool safe = true) => (GPRegister)Random.Shared.Next(safe ? (int)GPRegister.ReturnAddress : int.MaxValue);

    public static OperationCode RandomOpCode(bool safe = true) => (OperationCode)Random.Shared.Next(safe ? (int)OperationCode.StoreWordCoprocessor3 : int.MaxValue);

    public static FunctionCode RandomFuncCode(bool safe = true) => (FunctionCode)Random.Shared.Next(safe ? (int)FunctionCode.SetLessThanUnsigned : int.MaxValue);

    public static FloatFormat RandomFormat(HashSet<FloatFormat>? set) => set?.ElementAt(Random.Shared.Next(set.Count-1)) ?? FloatFormat.Single;
}
