// Avishai Dernis 2026

namespace Zarem.Emulator.TrapHandlers.Interfaces;

/// <summary>
/// A base class for a class that handles traps on behalf of the emulator.
/// </summary>
public interface ITrapHandler
{
    /// <summary>
    /// Defers trap handling to a custom implementation.
    /// </summary>
    void HandleTrap(ITrapContext context);
}
