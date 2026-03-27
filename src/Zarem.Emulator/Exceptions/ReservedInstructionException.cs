// Avishai Dernis 2026

namespace Zarem.Emulator.Exceptions;

/// <summary>
/// An exception thrown when an invalid instruction attempts to execute.
/// </summary>
public class ReservedInstructionException : EmulationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSyscallException"/> class.
    /// </summary>
    public ReservedInstructionException(ulong address)
        : base(address, "ReservedInstructionException")
    {
    }
}
