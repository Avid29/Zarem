// Avishai Dernis 2025

namespace Zarem.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum for pesudo-instruction types.
/// </summary>
public enum RiscVPseudoOp
{
    #pragma warning disable CS1591

    NoOperation,
    LoadImmediate,
    Move,
    LoadAddress,
        
    #pragma warning restore CS1591
}
