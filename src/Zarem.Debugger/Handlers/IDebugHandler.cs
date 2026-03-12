// Avishai Dernis 2026

using System;

namespace Zarem.Debugger.Handlers;

/// <summary>
/// An interface for an architecture specific debugger behavior handler.
/// </summary>
public interface IDebugHandler
{
    /// <summary>
    /// Gets the bytes that make a breakpoint instruction.
    /// </summary>
    public ReadOnlySpan<byte> BreakpointBytes { get; }

    /// <summary>
    /// Gets the size of instructions in the ISA.
    /// </summary>
    /// <remarks>
    /// 0 is variable size.
    /// </remarks>
    public uint InstructionSize { get; }
}
