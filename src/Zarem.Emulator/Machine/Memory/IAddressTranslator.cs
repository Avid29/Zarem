// Avishai Dernis 2026

using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Machine.Memory;

/// <summary>
/// An interface for a unit that translates addresses between physical and virtual memory.
/// </summary>
public interface IAddressTranslator
{
    /// <summary>
    /// Translates a virtual address to a physical address.
    /// </summary>
    /// <param name="virtualAddress">The virtual address to translate.</param>
    /// <returns>The physical address of the given virtual address.</returns>
    ulong Translate(ulong virtualAddress);

    /// <summary>
    /// Attempts to translate a virtual address to a physical address.
    /// </summary>
    /// <param name="virtualAddress">The virtual address to translate.</param>
    /// <param name="address">The physical address of the given virtual address, if mapped.</param>
    /// <returns>A <see cref="MemoryAccessResult"/> indicating the outcome of the operation.</returns>
    MemoryAccessResult TryTranslate(ulong virtualAddress, out ulong address);
}
