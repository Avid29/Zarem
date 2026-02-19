// Avishai Dernis 2026

using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.Models.Modules;
using Zarem.Models;

namespace Zarem.Emulator;

/// <summary>
/// An interface for an emulator.
/// </summary>
public interface IEmulator
{
    /// <summary>
    /// Gets the state of the emulator.
    /// </summary>
    EmulatorState State { get; set; }

    /// <summary>
    /// Loads an <see cref="IExecutableModule"/> to the interpreter's memory.
    /// </summary>
    /// <remarks>
    /// Also sets the program counter.
    /// </remarks>
    /// <param name="module">The module to load.</param>
    void Load(Module module);

    /// <summary>
    /// Starts the execution loop for the emulator.
    /// </summary>
    void Start();

    /// <summary>
    /// Resume the execution loop if paused.
    /// </summary>
    void Resume();

    /// <summary>
    /// Stops execution
    /// </summary>
    void Pause();

    /// <summary>
    /// Shuts down the emulation.
    /// </summary>
    void ShutDown();
}
