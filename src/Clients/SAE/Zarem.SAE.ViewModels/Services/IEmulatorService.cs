// Avishai Dernis 2026

using Zarem.Emulator.Models.Enums;

namespace Zarem.SAE.Services;

/// <summary>
/// An interface for a service to manage the emulator status.
/// </summary>
public interface IEmulatorService
{
    /// <summary>
    /// Gets the current state of the emulator.
    /// </summary>
    EmulatorState State { get; }

    /// <summary>
    /// Initializes the emulator with the necessary configuration and resources.
    /// </summary>
    void InitializeEmulator();

    /// <summary>
    /// Starts the emulator.
    /// </summary>
    void StartEmulator();

    /// <summary>
    /// Stops the emulator.
    /// </summary>
    void StopEmulator();
}
