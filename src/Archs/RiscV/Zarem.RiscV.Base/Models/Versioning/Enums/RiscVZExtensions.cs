// Avishai Dernis 2026

using System;
using Zarem.RiscV.Attributes;

namespace Zarem.RiscV.Models.Versioning.Enums;

/// <summary>
/// An enum for RISC-V MISA extensions.
/// </summary>
[Flags]
public enum RiscVZExtensions
{
#pragma warning disable CS1591

    None = 0,

    [RiscVExtension("Zifencei")] InstructionFetchFence = 0x1,
    [RiscVExtension("Zicsr")] ControlAndStatusRegisters = 0x2,
    [RiscVExtension("Zfh", misa: RiscVExtensions.SingleFloatingPoint)] HalfPrecisionFloatingPoint = 0x4,

#pragma warning restore CS1591
}
