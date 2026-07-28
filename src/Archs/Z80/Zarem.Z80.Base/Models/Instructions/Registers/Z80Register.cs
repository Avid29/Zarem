// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.Z80.Models.Instructions.Registers;

/// <summary>
/// An enum representing the general-purpose registers in Zilog Z80.
/// </summary>
public enum Z80Register
{
#pragma warning disable CS1591

    [Register("B")] B = 0,
    [Register("C")] C = 1,
    [Register("D")] D = 2,
    [Register("E")] E = 3,
    [Register("H")] H = 4,
    [Register("L")] L = 5,
    [Register("Hl")] HL = 6,
    [Register("A")] A = 7

#pragma warning restore CS1591
}
