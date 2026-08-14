// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zarem.Extensions.System;
using Zarem.Models.Enums;
using Zarem.Models.Interface;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// A base class for a <see cref="IParsedInstruction"/> implementation that is backed by a raw instruction struct.
/// </summary>
/// <typeparam name="TRaw"></typeparam>
public class ParsedInstructionBase<TRaw> : IParsedInstruction
    where TRaw : unmanaged, IInstruction
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
        // Scan to find realized size
        int size = 0;
        foreach (var inst in Instructions)
            size += inst.Length;

        // Allocate buffer
        byte[] bytes = new byte[size];
        Span<byte> destination = bytes;

        // Write instructions to the buffer
        int offset = 0;
        for (int i = 0; i < Instructions.Length; i++)
        {
            // Get the instruction and its length
            var instruction = Instructions[i];
            int length = instruction.Length;

            // Append the instruction and increment the offset
            destination[offset..].WriteEndianness(length, instruction, Endianness is Endianness.Little);
            offset += length;
        }

        return bytes;
    }
}
