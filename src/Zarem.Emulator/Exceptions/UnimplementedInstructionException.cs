// Avishai Dernis 2026

namespace Zarem.Emulator.Exceptions;

/// <summary>
/// An exception thrown when an unimplemented instruction attempts to execute.
/// </summary>
public class UnimplementedInstructionException : EmulationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnimplementedInstructionException"/> class.
    /// </summary>
    public UnimplementedInstructionException(ulong address)
        : base(address, "UnimplementedInstructionException")
    {
    }
}
