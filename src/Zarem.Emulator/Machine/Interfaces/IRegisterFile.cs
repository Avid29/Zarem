// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for accessing an <see cref="RegisterFile{T}"/> without a concrete type.
/// </summary>
public interface IRegisterFile
{
    /// <summary>
    /// Gets the number of registers in the register file.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    ulong this[int register] { get; set; }
}
