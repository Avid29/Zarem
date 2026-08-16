// Avishai Dernis 2026

using System;
using Zarem.RiscV.Attributes;

namespace Zarem.RiscV.Models.Versioning.Enums;

/// <summary>
/// An enum for RISC-V MISA extensions.
/// </summary>
[Flags]
public enum RiscVZExtensions : uint
{
#pragma warning disable CS1591

    None = 0,

    [RiscVExtension("Zba")] AddressGeneration = 1U << 0,
    [RiscVExtension("Zbb")] BasicBitManipulation = 1U << 1,
    [RiscVExtension("Zbc")] CarrylessMultiplication = 1U << 2,
    [RiscVExtension("Zbs")] SingleBitManipulation = 1U << 3,
    [RiscVExtension("Zifencei")] InstructionFetchFence = 1U << 4,
    [RiscVExtension("Zicsr")] ControlAndStatusRegisters = 1U << 5,
    [RiscVExtension("Zfh", misa: RiscVExtensions.SingleFloatingPoint)] HalfPrecisionFloatingPoint = 1U << 6,

#pragma warning restore CS1591
}
