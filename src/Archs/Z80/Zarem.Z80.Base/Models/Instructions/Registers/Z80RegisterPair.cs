// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.Z80.Models.Instructions.Registers;

/// <summary>
/// An enum representing a 16bit register in Zilog Z80.
/// </summary>
public enum Z80RegisterPair : byte
{
#pragma warning disable CS1591

    [Register("BC")] BC = 0,
    [Register("DE")] DE = 1,
    [Register("HL")] HL = 2,
    [Register("SP")] StackPointer = 3,
    [Register("AF")] AccumulatorFlags = 4,

#pragma warning restore CS1591
}
