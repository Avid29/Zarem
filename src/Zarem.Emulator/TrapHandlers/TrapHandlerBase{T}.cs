// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A base class for a class that handles traps on behalf of the emulator.
/// </summary>
public abstract class TrapHandlerBase<TTrap>
    where TTrap : Enum
{
    /// <summary>
    /// A method to direct a syscall to the appropriate interpretation.
    /// </summary>
    /// <param name="code">The syscall code.</param>
    protected abstract void HandleSyscall(uint code);

    /// <summary>
    /// A method to direct trap handling.
    /// </summary>
    /// <param name="trap">The type of trap that occurred.</param>
    protected abstract void HandleTrap(TTrap trap);
}
