// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// A base class for a <see cref="IParsedInstruction"/> implementation that is backed by a raw instruction struct.
/// </summary>
/// <typeparam name="TRaw"></typeparam>
public abstract class ParsedInstructionBase<TRaw> : IParsedInstruction
    where TRaw : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedInstructionBase{TRaw}"/> class.
    /// </summary>
    public ParsedInstructionBase(TRaw instruction, List<RelocationEntry>? references = null) : this([instruction], references)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedInstructionBase{TRaw}"/> class.
    /// </summary>
    public ParsedInstructionBase(TRaw[] instructions, List<RelocationEntry>? references = null)
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

    /// <inheritdoc/>
    public List<RelocationEntry>? References { get; }

    /// <inheritdoc/>
    public abstract byte[] RealizeBytes();
}
