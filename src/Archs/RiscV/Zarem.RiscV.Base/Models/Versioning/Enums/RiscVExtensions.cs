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

    None = 0,
    [JsonStringEnumMemberName("I")] Integers = 1 << 0,
    [JsonStringEnumMemberName("M")] Multiplication = 1 << 1,
    [JsonStringEnumMemberName("A")] Atomic = 1 << 2,
    [JsonStringEnumMemberName("F")] FloatingPoint = 1 << 3,
    [JsonStringEnumMemberName("D")] DoubleFloatingPoint = 1 << 4,
    [JsonStringEnumMemberName("C")] Compressed = 1 << 5,    

    // "G" is a shorthand for IMAFD + Zicsr + Zifencei
    General = Integers | Multiplication | Atomic | FloatingPoint | DoubleFloatingPoint,

#pragma warning restore CS1591

}
