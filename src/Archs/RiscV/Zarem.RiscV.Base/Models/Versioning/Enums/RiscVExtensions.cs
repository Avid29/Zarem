// Avishai Dernis 2026

using System;
using System.Text.Json.Serialization;
using Zarem.RiscV.Attributes;

namespace Zarem.RiscV.Models.Versioning.Enums;

/// <summary>
/// An enum for RISC-V lettered extensions.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVExtensions : uint
{
#pragma warning disable CS1591
    None = 0,

    [RiscVExtension("A")] Atomic = 1U << ('A'-'A'),
    [RiscVExtension("B", z: RiscVZExtensions.AddressGeneration | RiscVZExtensions.BasicBitManipulation | RiscVZExtensions.SingleBitManipulation)] BitManipulation = 1U << ('B' - 'A'),
    [RiscVExtension("C")] Compressed = 1U << ('C' - 'A'),
    [RiscVExtension("D", misa: SingleFloatingPoint)] DoubleFloatingPoint = 1U << ('D' - 'A'),
    [RiscVExtension("E")] Embedded = 1U << ('E' - 'A'),
    [RiscVExtension("F")] SingleFloatingPoint = 1U << ('F' - 'A'),
    [RiscVExtension("H")] Hypervisor = 1 << ('H' - 'A'),
    [RiscVExtension("I")] Integers = 1 << ('I' - 'A'),
    [RiscVExtension("J")] DynamicTranslatedLanguages = 1 << ('J' - 'A'),
    [RiscVExtension("K")] ScalarCryptography = 1 << ('K' - 'A'),
    [RiscVExtension("L", misa: SingleFloatingPoint)] DecimalFloatingPoint = 1 << ('L' - 'A'),
    [RiscVExtension("M")] Multiplication = 1 << ('M' - 'A'),
    [RiscVExtension("N")] UserLevelInterrupts = 1 << ('N' - 'A'),
    [RiscVExtension("P")] PackedSIMD = 1 << ('P' - 'A'),
    [RiscVExtension("Q", misa: SingleFloatingPoint | DoubleFloatingPoint)] QuadrupleFloatingPoint = ('Q' - 'A'),
    [RiscVExtension("S")] SuperVisorMode = 1 << ('S' - 'A'),
    [RiscVExtension("T")] TransactionalMemory = 1 << ('T' - 'A'),
    [RiscVExtension("V")] Vectors = 1 << ('V' - 'A'),

    // Shorthand Alias
    [RiscVExtension("G",
        misa: Integers | Multiplication | Atomic | SingleFloatingPoint | DoubleFloatingPoint,
        z: RiscVZExtensions.ControlAndStatusRegisters | RiscVZExtensions.InstructionFetchFence)]
    General = 0,
#pragma warning restore CS1591
}
