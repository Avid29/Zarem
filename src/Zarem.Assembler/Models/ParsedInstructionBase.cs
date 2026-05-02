// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zarem.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// A base class for a <see cref="IParsedInstruction"/> implementation that is backed by a raw instruction struct.
/// </summary>
/// <typeparam name="TRaw"></typeparam>
public class ParsedInstructionBase<TRaw> : IParsedInstruction
    where TRaw : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedInstructionBase{TRaw}"/> class.
    /// </summary>
    public ParsedInstructionBase(TRaw instruction, IReadOnlyList<RelocationEntry>? references = null) : this([instruction], references)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedInstructionBase{TRaw}"/> class.
    /// </summary>
    public ParsedInstructionBase(TRaw[] instructions, IReadOnlyList<RelocationEntry>? references = null)
    {
        Instructions = instructions;
        References = references;
    }

    /// <summary>
    /// Gets the parsed instructions that should be emitted for this parsed instruction.
    /// </summary>
    /// <remarks>
    /// Might be more than one instruction if the original instruction was a pseudo-instruction that expands into multiple real instructions.
    /// </remarks>
    public TRaw[] Instructions { get; }

    /// <summary>
    /// Gets the endianness of the instruction.
    /// </summary>
    public required Endianness Endianness { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<RelocationEntry>? References { get; }

    /// <inheritdoc/>
    public byte[] RealizeBytes()
    {
        byte[] bytes = new byte[Instructions.Length * sizeof(uint)];
        Span<byte> destination = bytes;

        for (int i = 0; i < Instructions.Length; i++)
        {
            // TODO: Handle instructions of different sizes (e.g. 16-bit compressed instructions in RISC-V).
            var raw = Unsafe.As<TRaw, uint>(ref Instructions[i]);
            if (Endianness is Endianness.Little)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(destination[(i * 4)..], raw);
            }
            else
            {
                BinaryPrimitives.WriteUInt32BigEndian(destination[(i * 4)..], raw);
            }
        }

        return bytes;
    }
}
