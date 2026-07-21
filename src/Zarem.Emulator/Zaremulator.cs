// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Threading;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Localization;
using Zarem.Models;

namespace Zarem.Emulator;

/// <summary>
/// An emulator class that wraps an <see cref="IComputer"/> for emulation.
/// </summary>
public class Zaremulator : IDisposable
{
    private readonly ManualResetEventSlim _runGate = new(false);
    private CancellationTokenSource? _cts;
    private Thread? _thread;

    /// <summary>
    /// An event invoked when the emulator state changes.
    /// </summary>
    public event EventHandler<EmulatorState>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zaremulator"/> class.
    /// </summary>
    public Zaremulator(IComputer computer)
    {
        Computer = computer;

        Computer.ShutdownRequested += Computer_ShutdownRequested;
    }

    private void Computer_ShutdownRequested(object? sender, EventArgs e) => ShutDown();

    /// <summary>
    /// Gets the emulated computer info.
    /// </summary>
    public IComputer Computer { get; }

    /// <inheritdoc/>
    public EmulatorState State
    {
        get;
        set
        {
            field = value;
            StateChanged?.Invoke(this, value);
        }
    } = EmulatorState.Stopped;

    /// <inheritdoc/>
    public void Load(Module module)
    {
        Computer.Load(module);

        State = EmulatorState.Ready;
    }

    /// <inheritdoc/>
    public void Start()
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
    public void Resume()
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
    public void Pause()
    {
        // Schedule pause
        State = EmulatorState.Pausing;
        _cts?.Cancel();
        _runGate.Reset();
    }

    /// <inheritdoc/>
    public void ShutDown()
    {
        // Schedule the shutdown
        State = EmulatorState.Stopping;
        _cts?.Cancel();
        _runGate.Set(); // The thread must run to exit
    }

    /// <summary>
    /// The loop that progresses the emulation while running.
    /// </summary>
    protected void ExecutionLoop()
    {
        try
        {
            while (State is not EmulatorState.Stopping)
            {
                // Blocks the thread while Paused
                _runGate.Wait();

                // Check if we exited the wait because of a Stop request
                if (State is EmulatorState.Stopping)
                    break;

                if (State is EmulatorState.Running)
                {
                    // Create a fresh token for this run session
                    _cts = new CancellationTokenSource();

                    // Hand control to the computer.
                    // It will loop internally until _cts.Cancel() is called.
                    Computer.Run(_cts.Token);
                }

                // Transition logic for the state machine
                if (State is EmulatorState.Pausing)
                    State = EmulatorState.Paused;
            }
        }
        catch (EmulationException e)
        {
            Console.WriteLine($"\n{e.Message}");
        }
        catch (Exception e)
        {
            var localizer = new Localizer("Zarem.Emulator.Resources.Messages", typeof(Zaremulator).Assembly);
            Console.WriteLine($"\n{localizer["ExceptionOccurred", e]}");
        }

        State = EmulatorState.Stopped;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ShutDown();
        Computer.Dispose();
    }
}
