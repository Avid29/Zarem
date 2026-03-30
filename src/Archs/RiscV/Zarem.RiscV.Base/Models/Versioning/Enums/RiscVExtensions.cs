// Avishai Dernis 2026

using System;

namespace Zarem.Models.Versioning.Enums;

/// <summary>
/// An enum for RISC-V extensions groups.
/// </summary>
[Flags]
public enum RiscVExtensions : uint
{
#pragma warning disable CS1591
    None = 0,
    Integers = 1 << 0,       
    Multiplication = 1 << 1,
    Atomics = 1 << 2,       
    FloatingPoint = 1 << 3, 
    Double = 1 << 4,        
    Compressed = 1 << 5,    

    // "G" is a shorthand for IMAFD + Zicsr + Zifencei
    General = Integers | Multiplication | Atomics | FloatingPoint | Double,
#pragma warning restore CS1591
}
