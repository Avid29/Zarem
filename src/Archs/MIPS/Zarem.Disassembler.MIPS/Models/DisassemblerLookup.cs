// Avishai Dernis 2026

namespace Zarem.Disassembler.Models;

/// <summary>
/// A type describing the info to lookup an instruction in a disassembler instruction table.
/// </summary>
public readonly struct DisassemblerLookup
{
    /// <summary>
    /// Gets the primary 6-bit operation code (bits 31:26) from the instruction word.
    /// </summary>
    public byte OpCode { get; }

    /// <summary>
    /// Gets the primary sub-identifier used for disambiguation. 
    /// Depending on the <see cref="OpCode"/>, this represents the 'func' field (bits 5:0) for R-Type, 
    /// the 'rt' field (bits 20:16) for RegImm, or the 'rs' field (bits 25:21) for Coprocessors.
    /// </summary>
    public byte Function { get; }

    /// <summary>
    /// Gets a secondary sub-identifier, typically used for Coprocessor functional operations (bits 5:0).
    /// Defaults to 255 if not applicable to the instruction type.
    /// </summary>
    public byte SecondaryFunction { get; }

    /// <summary>
    /// Gets a specific register index that must be present in the 'rd' field (bits 15:11) for a valid match.
    /// Used to distinguish specialized instructions like 'eret' (rd=0) from 'eretnc' (rd=1).
    /// </summary>
    public byte? FixedRD { get; }

    /// <summary>
    /// Gets a value indicating whether the lookup should prioritize the functional 'func' bits [5:0] 
    /// over the 'rs' format bits [25:21] when resolving Coprocessor 1 (Floating Point) instructions.
    /// </summary>
    public bool IsFloatFunc { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisassemblerLookup"/> struct.
    /// </summary>
    public DisassemblerLookup(byte op, byte func = 0, byte sec = 255, byte? fixedRd = null, bool isFloatFunc = false)
    {
        OpCode = op;
        Function = func;
        SecondaryFunction = sec;
        FixedRD = fixedRd;
        IsFloatFunc = isFloatFunc;
    }
}
