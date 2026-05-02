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
    private readonly MipsInstruction[] _instructions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsInstruction instruction, List<RelocationEntry>? references = null)
    {
        _instructions = instruction;
        References = references;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsInstruction[] instructions, List<RelocationEntry>? references = null)
    {
        _instructions = instructions;
        References = references;
    }

    /// <inheritdoc/>
    public List<RelocationEntry>? References { get; }
    
    /// <inheritdoc/>
    public byte[] RealizeBytes()
    {
        byte[] bytes = new byte[_instructions.Length * sizeof(uint)];
        Span<byte> destination = bytes;

        for (int i = 0; i < _instructions.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(destination[(i * 4)..], (uint)_instructions[i]);
        }

        return bytes;
    }
}
