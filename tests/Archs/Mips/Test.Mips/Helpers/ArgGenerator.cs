// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.Functions;

namespace Test.Mips.Helpers;

public class ArgGenerator
{
    private static Random Rnd => Random.Shared;

    // Use the full range of short (-32768 to 32767)
    public static short RandomImmediate(bool safe = true)
            => safe ? (short)Rnd.Next(short.MinValue, short.MaxValue + 1)
                    : (short)Rnd.Next(int.MinValue, int.MaxValue);

    // Shifts are exactly 5 bits. Next(32) gives 0-31.
    public static byte RandomShift(bool safe = true)
            => (byte)(safe ? Rnd.Next(0, 32) : Rnd.Next(32, 256));

    public static int RandomOffset(bool safe = true)
    {
        if (!safe)
        {
            return Rnd.Next(int.MinValue, int.MaxValue);
        }

        // Range: [-32768, 32767] but masked for 4-byte alignment
        return Rnd.Next(short.MinValue, short.MaxValue + 1) & ~0b11;
    }

    public static uint RandomAddress(bool safe = true)
    {
        if (!safe)
        {
            return (uint)Rnd.NextInt64(0, uint.MaxValue);
        }

        // 26-bit range: [0, 67108863]
        return (uint)Rnd.Next(0, 1 << 26) & ~0b11u;
    }

    public static MipsGpRegister RandomRegister(bool safe = true)
            => (MipsGpRegister)(safe ? Rnd.Next(0, 32) : Rnd.Next(32, 256));

    public static MipsOpCode RandomOpCode(bool safe = true)
            => (MipsOpCode)(safe ? Rnd.Next(0, 64) : Rnd.Next(64, 256));

    public static FunctionCode RandomFuncCode(bool safe = true)
            => (FunctionCode)(safe ? Rnd.Next(0, 64) : Rnd.Next(64, 256));

    public static MipsFloatFormat RandomFormat(HashSet<MipsFloatFormat>? set) => set?.ElementAt(Random.Shared.Next(set.Count-1)) ?? MipsFloatFormat.Single;
}
