// Avishai Dernis 2024

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// An instruction as parsed by the <see cref="MipsInstructionParser"/>.
/// </summary>
public class MipsParsedInstruction : IParsedInstruction
{
    private readonly MipsInstruction? _real;
    private readonly MipsPseudoInstruction? _pseudo;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsInstruction instruction, List<RelocationEntry>? references = null)
    {
        _real = instruction;
        References = references;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsPseudoInstruction instruction, List<RelocationEntry>? references = null)
    {
        _pseudo = instruction;
        References = references;
    }

    /// <inheritdoc/>
    public List<RelocationEntry>? References { get; }

    /// <summary>
    /// Gets whether or not the parsed instruction was a pseudo instruction.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_real))]
    [MemberNotNullWhen(true, nameof(_pseudo))]
    public bool IsPseduoInstruction => _real is null;

    /// <summary>
    /// Gets the parsed instruction implemented exlcusively in real instructions.
    /// </summary>
    public MipsInstruction[] Realize()
    {
        if (!IsPseduoInstruction)
            return [_real.Value];

        return _pseudo.Value.Expand();
    }
    
    /// <inheritdoc/>
    public byte[] RealizeBytes()
    {
        var instructions = Realize();
        byte[] bytes = new byte[instructions.Length * sizeof(uint)];
        Span<byte> destination = bytes;

        for (int i = 0; i < instructions.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(destination[(i * 4)..], (uint)instructions[i]);
        }

        return bytes;
    }
}
