// Avishai Dernis 2026

namespace Zarem.Mips.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for coprocessor1 registers.
/// </summary>
public enum CP1CRegisters
{
#pragma warning disable CS1591

    Implementation = 0,
    ConditionCodes = 25,
    Exceptions = 26,
    Enables = 28,
    ControlStatus = 31,

#pragma warning restore CS1591
}
