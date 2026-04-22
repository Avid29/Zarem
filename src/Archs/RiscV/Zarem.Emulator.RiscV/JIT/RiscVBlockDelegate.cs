// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Machine.Enums;

namespace Zarem.Emulator.JIT;

/// <summary>
/// Represents a compiled block of MIPS instructions.
/// </summary>
/// <typeparam name="T">The register width (uint or ulong).</typeparam>
/// <param name="cpu">The CPU instance to operate on.</param>
/// <param name="trap">The trap which caused the block to exit.</param>
/// <returns>The Program Counter where execution should continue.</returns>
public delegate T RiscVBlockDelegate<T>(RiscVJitCpu<T> cpu, out RiscVTrap trap)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;
