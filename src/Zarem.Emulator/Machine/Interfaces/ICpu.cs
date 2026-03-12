// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for the CPU state.
/// </summary>
public interface ICpu
{
    /// <summary>
    /// Gets the CPU architecture's name.
    /// </summary>
    string ArchitectureName { get; }

    /// <summary>
    /// Gets or sets the current program counter.
    /// </summary>
    ulong ProgramCounter { get; set; }

    ///// <summary>
    ///// Gets the register info for the CPU.
    ///// </summary>
    // TODO: Add debugger interface to expose register info
    //IRegisterGroup Registers { get; }
}
