// Adam Dernis 2024

using System.Diagnostics.CodeAnalysis;
using Zarem.Assembler.Parsers;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models.Instructions;

/// <summary>
/// An instruction as parsed by the <see cref="MIPSInstructionParser"/>.
/// </summary>
public class MIPSParsedInstruction : IParsedInstruction
{
    private readonly MIPSInstruction? _real;
    private readonly PseudoInstruction? _pseudo;

    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSParsedInstruction"/> class.
    /// </summary>
    public MIPSParsedInstruction(MIPSInstruction instruction, RelocationEntry? reference = null)
    {
        _real = instruction;
        Reference = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSParsedInstruction"/> class.
    /// </summary>
    public MIPSParsedInstruction(PseudoInstruction instruction, RelocationEntry? reference = null)
    {
        _pseudo = instruction;
        Reference = reference;
    }

    /// <summary>
    /// Gets the symbol referenced, or null if none.
    /// </summary>
    public RelocationEntry? Reference { get; }

    /// <summary>
    /// Gets whether or not the parsed instruction was a pseudo instruction.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_real))]
    [MemberNotNullWhen(true, nameof(_pseudo))]
    public bool IsPseduoInstruction => _real is null;

    /// <summary>
    /// Gets the parsed instruction implemented exlcusively in real instructions.
    /// </summary>
    public MIPSInstruction[] Realize()
    {
        if (!IsPseduoInstruction)
            return [_real.Value];

        return _pseudo.Value.Expand();
    }
    
    /// <inheritdoc/>
    public byte[] RealizeBytes()
    {
        // TODO: Realize bytes properly
        return [];
    }
}
