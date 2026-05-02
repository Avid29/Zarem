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
public class MipsParsedInstruction : ParsedInstructionBase<MipsInstruction>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsInstruction instruction, List<RelocationEntry>? references = null) : base(instruction, references)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsParsedInstruction"/> class.
    /// </summary>
    public MipsParsedInstruction(MipsInstruction[] instructions, List<RelocationEntry>? references = null) : base(instructions, references)
    {
    }

    /// <inheritdoc/>
    public override byte[] RealizeBytes()
    {
        byte[] bytes = new byte[Instructions.Length * sizeof(uint)];
        Span<byte> destination = bytes;

        for (int i = 0; i < Instructions.Length; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(destination[(i * 4)..], (uint)Instructions[i]);
        }

        return bytes;
    }
}
