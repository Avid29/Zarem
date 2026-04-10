// Avishai Dernis 2025

using System;
using Zarem.Models.Instructions.Enums.Registers;

namespace Test.RiscV.Helpers;

public class ArgGenerator
{
    private static Random Rnd => Random.Shared;

    public static int RandomImm12(bool safe = true)
        => safe ? Rnd.Next(-2048, 2048) : Rnd.Next(int.MinValue, int.MaxValue);

    public static int RandomImm20(bool safe = true)
        => safe ? Rnd.Next(-(1 << 19), 1 << 19) : Rnd.Next(int.MinValue, int.MaxValue);

    public static RiscVGpRegister RandomRegister(bool safe = true)
        => (RiscVGpRegister)(safe ? Rnd.Next(0, 32) : Rnd.Next(32, 256));

    public static byte RandomShamt(bool safe = true)
        => (byte)(safe ? Rnd.Next(0, 32) : Rnd.Next(32, 256));

    public static int RandomBranchOffset(bool safe = true)
    {
        if (!safe)
        {
            return Rnd.Next(int.MinValue, int.MaxValue);
        }

        // Range: [-4096, 4094] aligned to 2
        return Rnd.Next(-4096, 4096) & ~0b1;
    }

    public static int RandomJumpOffset(bool safe = true)
    {
        if (!safe)
        {
            return Rnd.Next(int.MinValue, int.MaxValue);
        }

        // Range: [-1048576, 1048574] aligned to 2
        return Rnd.Next(-(1 << 20), 1 << 20) & ~0b1;
    }

    public static byte RandomOpCode(bool safe = true)
        => (byte)(safe ? Rnd.Next(0, 128) : Rnd.Next(128, 256));

    public static byte RandomFunct3(bool safe = true)
        => (byte)(safe ? Rnd.Next(0, 8) : Rnd.Next(8, 256));

    public static byte RandomFunct7(bool safe = true)
        => (byte)(safe ? Rnd.Next(0, 128) : Rnd.Next(128, 256));
}
