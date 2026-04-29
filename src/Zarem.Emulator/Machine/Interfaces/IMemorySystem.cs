// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for a computer's memory system.
/// </summary>
public interface IMemorySystem : IMemoryAccessor
{
    /// <summary>
    /// Gets the physical memory <see cref="PhysicalBus"/>.
    /// </summary>
    PhysicalBus Physical { get; }

    /// <summary>
    /// Gets the virtual memory <see cref="IVirtualMemoryAccessor"/>.
    /// </summary>
    IVirtualMemoryAccessor Virtual { get; }
}
