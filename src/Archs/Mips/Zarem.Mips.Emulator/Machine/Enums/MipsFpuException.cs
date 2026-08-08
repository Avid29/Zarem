// Avishai Dernis 2026

using System;

namespace Zarem.Mips.Emulator.Machine.Enums;

/// <summary>
/// Bit flags representing standard MIPS FPU IEEE 754 exception conditions.
/// </summary>
[Flags]
public enum MipsFpuException : byte
{
#pragma warning disable CS1591
    
    None,
    Inexact,
    Underflow,
    Overflow,
    DivisionByZero,
    InvalidOperation,
    UnimplementedOp,

#pragma warning restore CS1591
}
