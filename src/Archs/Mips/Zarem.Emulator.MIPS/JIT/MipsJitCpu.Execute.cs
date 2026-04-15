// Avishai Dernis 2026

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.TrapHandlers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// Represents a compiled block of MIPS instructions.
/// </summary>
/// <typeparam name="T">The register width (uint or ulong).</typeparam>
/// <param name="cpu">The CPU instance to operate on.</param>
/// <returns>The Program Counter where execution should continue.</returns>
public delegate T MipsBlockDelegate<T>(MipsJitCpu<T> cpu)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;

public partial class MipsJitCpu<T> : IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    // Cache mapping a PC to its compiled IL block.
    private readonly MipsBlockCache<T> _blockCache = new();
    private readonly MipsJitCompiler<T> _jitCompiler;

    /// <inheritdoc/>
    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Look up the current block
            if (!_blockCache.TryGet(ProgramCounter, out var compiledBlock))
            {
                // Cache Miss. Compile the basic block starting at ProgramCounter
                compiledBlock = _jitCompiler.CompileBlock(ProgramCounter);
                _blockCache.Store(ProgramCounter, compiledBlock);
            }

            // Execute the block, and update the PC to the next block start
            ProgramCounter = compiledBlock(this);
        }
    }

    /// <summary>
    /// Handles a trap.
    /// </summary>
    public void HandleTrap(int trapCode, T currentPc)
    {
        var trap = (MipsTrap)trapCode;

        // Sync the PC so the interpreter/debugger knows where we are
        ProgramCounter = currentPc;

        if (trap is MipsTrap.None)
            return;

        // Breakpoints are handled by the debugger upon the trap occurring event
        // The host also handles every kind of trap if that's what the config specifies
        if (trap is MipsTrap.Breakpoint && BreakpointHit is not null)
        {
            // Only wait if a debugger is attached
            var eventArgs = new BreakpointHitEventArgs();
            BreakpointHit.Invoke(this, eventArgs);
            eventArgs.Wait();
        }
        else if (Config.TrapHost is not null)
        {
            // The host handled the trap, do not emulate it
            // Breakpoints are always handled by the host
            Config.TrapHost.HandleTrap(new MipsTrapContext(this, (ulong)trap));
        }
        else
        {
            CoProcessor0.EnterTrap(trap, ProgramCounter, DelaySlot.HasValue);
            ProgramCounter = CoProcessor0.ExceptionVector;
        }
    }
}
