// Avishai Dernis 2026

namespace Zarem.Emulator.Exceptions;

/// <summary>
/// An exception thrown an invalid syscall is made.
/// </summary>
public class InvalidSyscallException : EmulationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSyscallException"/> class.
    /// </summary>
    public InvalidSyscallException(ulong address, ulong syscallId)
        : base(address, "InvalidSyscallException", syscallId)
    {
        SyscallId = syscallId;
    }

    /// <summary>
    /// Gets the id of the syscall that was made.
    /// </summary>
    public ulong SyscallId { get; }
}
