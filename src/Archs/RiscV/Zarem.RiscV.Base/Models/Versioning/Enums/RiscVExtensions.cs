// Avishai Dernis 2026

using System;
using System.Text.Json.Serialization;

namespace Zarem.Models.Versioning.Enums;

/// <summary>
/// An enum for RISC-V extensions groups.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiscVExtensions : uint
{
#pragma warning disable CS1591

    [JsonStringEnumMemberName("I")] Integers = 0x0,
    [JsonStringEnumMemberName("A")] Atomic = 0x1,
    [JsonStringEnumMemberName("B")] BitManipulation = 0x2,
    [JsonStringEnumMemberName("C")] Compressed = 0x4,
    [JsonStringEnumMemberName("D")] DoubleFloatingPoint = 0x8,
    [JsonStringEnumMemberName("F")] SingleFloatingPoint = 0x10,
    [JsonStringEnumMemberName("H")] Hypervisor = 0x20,
    [JsonStringEnumMemberName("J")] DynamicTranslatedLanguages = 0x40,
    [JsonStringEnumMemberName("L")] DecimalFloatingPoint = 0x80,
    [JsonStringEnumMemberName("M")] Multiplication = 0x100,
    [JsonStringEnumMemberName("N")] UserLevelInterrupts = 0x200,
    [JsonStringEnumMemberName("P")] PackedSIMD = 0x400,
    [JsonStringEnumMemberName("Q")] QuadrupleFloatingPoint = 0x800,
    [JsonStringEnumMemberName("S")] SuperVisorMode = 0x1000,
    [JsonStringEnumMemberName("T")] TransactionalMemory = 0x2000,
    [JsonStringEnumMemberName("T")] Vectors = 0x4000,

    [JsonStringEnumMemberName("Zifencei")] InstructionFetchFence = 0x8000,
    [JsonStringEnumMemberName("Zicsr")] ControlAndStatusRegisters = 0x1_0000,
    [JsonStringEnumMemberName("Zfh")] HalfPrecisionFloatingPoint = 0x2_0000,

    [JsonStringEnumMemberName("G")] General = Integers | Multiplication | Atomic | SingleFloatingPoint | DoubleFloatingPoint | InstructionFetchFence | ControlAndStatusRegisters,
#pragma warning restore CS1591
}
