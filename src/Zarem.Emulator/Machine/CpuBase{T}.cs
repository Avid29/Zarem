// Avishai Dernis 2026

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
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
        private set
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
    public void Run(CancellationToken ct)
    {
        long totalInstructions = 0;
        var stopwatch = Stopwatch.StartNew();
        long lastReportTime = 0;

        while (!ct.IsCancellationRequested)
        {
            var executedInstruction = ExecutionLoop();

            // Update instruction count
            totalInstructions += executedInstruction;

            // Speed Check: Every 1000ms (1 second)
            long currentTime = stopwatch.ElapsedMilliseconds;
            if (currentTime - lastReportTime >= 1000)
            {
                double seconds = (currentTime - lastReportTime) / 1000.0;
                MeasuredSpeed = totalInstructions / seconds;

                // Reset for next interval
                totalInstructions = 0;
                lastReportTime = currentTime;
            }
        }
    }

    /// <summary>
    /// The base dispatch of the execution loop.
    /// </summary>
    /// <remarks>
    /// This method itself does NOT loop.
    /// </remarks>
    /// <returns>The number of instructions executed in the loop step.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract long ExecutionLoop();

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        RegisterFile.Dispose();
    }
}
