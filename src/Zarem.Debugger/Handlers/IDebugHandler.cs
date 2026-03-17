// Avishai Dernis 2026

using System;
using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Debugger.Handlers;

/// <summary>
/// An interface for an architecture specific debugger behavior handler.
/// </summary>
public interface IDebugHandler
{
    /// <summary>
    /// Gets the bytes that make a breakpoint instruction.
    /// </summary>
    ReadOnlySpan<byte> BreakpointBytes { get; }

    /// <summary>
    /// Gets the size of instructions in the ISA.
    /// </summary>
    /// <remarks>
    /// 0 is variable size.
    /// </remarks>
    uint InstructionSize { get; }

    /// <summary>
    /// Gets the step address for a given computer's state.
    /// </summary>
    /// <remarks>
    /// Step is always the next instruction that will execute.
    /// </remarks>
    ulong GetStepAddress(IComputer computer);

    /// <summary>
    /// Gets the step over address for a given computer's state.
    /// </summary>
    /// <remarks>
    /// Step over will skip jumps and branches (including their delay slots in MIPS).
    /// </remarks>
    ulong GetStepOverAddress(IComputer computer);

    /// <summary>
    /// Gets the step out address for a given computer's state.
    /// </summary>
    /// <remarks>
    /// The step out address is the return address.
    /// </remarks>
    ulong GetStepOutAddress(IComputer computer);

    /// <summary>
    /// Gets a new <see cref="IDebugViewer"/> for the given computer.
    /// </summary>
    IDebugViewer? GetDebugViewer(IComputer computer);
}
