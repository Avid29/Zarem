// Avishai Dernis 2026

using System.Threading.Tasks;
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
    Task InitializeEmulatorAsync();

    /// <summary>
    /// Starts the emulator.
    /// </summary>
    Task StartEmulatorAsync();

    /// <summary>
    /// Stops the emulator.
    /// </summary>
    void StopEmulator();

    /// <summary>
    /// Prompts the user to select a BIOS file, and saves it in settings.
    /// </summary>
    /// <returns>The selected BIOS file path.</returns>
    Task<string?> ConfigureBiosAsync();
}
