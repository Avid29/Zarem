// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Models.Enums;
using Zarem.Models;

namespace Zarem.Emulator;

/// <summary>
/// A base class for an emulator.
/// </summary>
/// <typeparam name="TConfig">The emulator configuration info</typeparam>
public abstract class Emulator<TConfig> : IEmulator
    where TConfig : EmulatorConfig
{
    private readonly ManualResetEventSlim _runGate = new(false);
    private Thread? _thread;

    /// <summary>
    /// An event invoked when the emulator state changes.
    /// </summary>
    public event EventHandler<EmulatorState>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Emulator{TConfig}"/> class.
    /// </summary>
    public Emulator(TConfig config)
    {
        Config = config;
    }

    /// <summary>
    /// Gets or sets the emulators configuration.
    /// </summary>
    public TConfig Config { get; }

    /// <inheritdoc/>
    public EmulatorState State
    {
        get => field;
        set
        {
            field = value;
            StateChanged?.Invoke(this, value);
        }
    } = EmulatorState.Stopped;

    /// <inheritdoc/>
    public abstract void Load(Module module);

    /// <inheritdoc/>
    public virtual void Start()
    {
        // Nothing to be done
        if (State is EmulatorState.Running)
            return;

        // Forward to resume
        if (State is EmulatorState.Paused)
        {
            Resume();
            return;
        }

        // Execution can only be started from ready
        Guard.IsTrue(State is EmulatorState.Ready);

        // Create and begin the thread
        _thread = new Thread(ExecutionLoop)
        {
            IsBackground = true,
        };
        _thread.Start();

        // Update the 
        State = EmulatorState.Running;
        _runGate.Set();
    }

    /// <inheritdoc/>
    public virtual void Resume()
    {
        // Nothing to be done
        if (State is EmulatorState.Running)
            return;

        // Execution can only be resumed from being paused
        Guard.IsTrue(State is EmulatorState.Paused);

        State = EmulatorState.Running;
        _runGate.Set();
    }

    /// <inheritdoc/>
    public virtual void Pause()
    {
        // Schedule pause
        State = EmulatorState.Pausing;
        _runGate.Reset();
    }

    /// <inheritdoc/>
    public virtual void ShutDown()
    {
        // Schedule the shutdown
        State = EmulatorState.Stopping;
        _runGate.Set(); // The thread must run to exit
    }

    /// <summary>
    /// Steps a tick in the processor.
    /// </summary>
    protected abstract void Tick();

    /// <summary>
    /// The loop that progresses the emulation while running.
    /// </summary>
    protected virtual void ExecutionLoop()
    {
        while (State is not EmulatorState.Stopping)
        {
            // Wait here if paused
            _runGate.Wait();

            // Loop ticks while running
            while (State is EmulatorState.Running)
                Tick();

            // Complete pausing transition
            if (State is EmulatorState.Pausing)
                State = EmulatorState.Paused;
        }

        // Complete the shutdown 
        State = EmulatorState.Stopped;
        _thread?.Join();
    }
}
