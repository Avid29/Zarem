// Avishai Dernis 2024

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// An instruction as parsed by the <see cref="RiscVInstructionParser"/>.
/// </summary>
public class RiscVParsedInstruction : ParsedInstructionBase<RiscVInstruction>, IParsedInstruction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVParsedInstruction"/> class.
    /// </summary>
    public RiscVParsedInstruction(RiscVInstruction instruction, List<RelocationEntry>? references = null) : base(instruction, references)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVParsedInstruction"/> class.
    /// </summary>
    public RiscVParsedInstruction(RiscVInstruction[] instructions, List<RelocationEntry>? references = null) : base(instructions, references)
    {
    }

    /// <inheritdoc/>
    public override byte[] RealizeBytes()
    {
        byte[] bytes = new byte[Instructions.Length * sizeof(uint)];
        Span<byte> destination = bytes;

        for (int i = 0; i < Instructions.Length; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(i * 4)..], (uint)Instructions[i]);
        }

        return bytes;
    }
}
