// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Emulator.Models.Enums;
using Zarem.Localization;
using Zarem.Models;

namespace Zarem.Emulator;

/// <summary>
/// An emulator class that wraps an <see cref="IComputer"/> for emulation.
/// </summary>
public class Zaremulator
{
    private readonly ManualResetEventSlim _runGate = new(false);
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
        get => field;
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
        _runGate.Reset();
    }

    /// <inheritdoc/>
    public void ShutDown()
    {
        // Schedule the shutdown
        State = EmulatorState.Stopping;
        _runGate.Set(); // The thread must run to exit
    }

    /// <summary>
    /// The loop that progresses the emulation while running.
    /// </summary>
    protected void ExecutionLoop()
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
        long totalInstructions = 0;
#endif
        try
        {
            while (State is not EmulatorState.Stopping)
            {
                // Wait here if paused
                _runGate.Wait();

                // Loop ticks while running
                while (State is EmulatorState.Running)
                {
                    Computer.Tick();

#if DEBUG
                    totalInstructions++;

                    if (sw.ElapsedMilliseconds > 500)
                    {
                        double mips = (totalInstructions / (sw.Elapsed.TotalSeconds * 1000000.0));
                        Debug.WriteLine($"{mips} MIPS");
                        totalInstructions = 0;
                        sw.Restart();
                    }
                }
#endif

                // Complete pausing transition
                if (State is EmulatorState.Pausing)
                    State = EmulatorState.Paused;
            }
        }
        catch
        {
            var localizer = new Localizer("Zarem.Emulator.Resources.Messages", typeof(Zaremulator).Assembly);
            Console.WriteLine(localizer["ExceptionOccurred"]);
        }

        // Complete the shutdown,
        // or handle exception
        State = EmulatorState.Stopped;
        _thread?.Join();
    }
}
