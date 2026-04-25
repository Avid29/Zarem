// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Threading;
using Zarem.Emulator.Events;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A base class for an emulated CPU.
/// </summary>
public abstract class CpuBase<T> : ICpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <inheritdoc/>
    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <inheritdoc/>
    public event EventHandler? ShutdownRequested;

    /// <inheritdoc/>
    public abstract string ArchitectureName { get; }

    /// <inheritdoc/>
    public abstract Endianness Endianness { get; }

    /// <inheritdoc/>
    public T ProgramCounter { get; protected set; }

    /// <inheritdoc/>
    ulong ICpu.ProgramCounter
    {
        get => ulong.CreateTruncating(ProgramCounter);
        set => ProgramCounter = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public abstract RegisterFile<T> RegisterFile { get; }

    /// <inheritdoc/>
    IRegisterFile ICpu.RegisterFile => RegisterFile;

    /// <inheritdoc/>
    public abstract MemorySystem Memory { get; }

    /// <inheritdoc/>
    public double MeasuredSpeed
    {
        get;
        protected set
        {
            field = value;
            Console.WriteLine($"Speed: {value / 1_000_000:F2} MHz");
        }
    }

    /// <inheritdoc/>
    public void RequestShutdown() => ShutdownRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Attempts to invoke a breakpoint.
    /// </summary>
    /// <returns>True if successfully invoked, false otherwise.</returns>
    protected bool InvokeBreakpoint()
    {
        // Only wait if a debugger is attached
        if (BreakpointHit is null)
            return false;

        var eventArgs = new BreakpointHitEventArgs();
        BreakpointHit.Invoke(this, eventArgs);
        eventArgs.Wait();
        return true;
    }

    /// <inheritdoc/>
    public abstract void Run(CancellationToken ct);

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        RegisterFile.Dispose();
    }
}
