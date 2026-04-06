// Avishai Dernis 2026

using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.TrapHandlers.Interfaces;

/// <summary>
/// An interface for the relevant registers when handling a trap. This is used to abstract the trap context from the architecture, so that the trap handlers can be implemented in a more generic way.
/// </summary>
public interface ITrapContext
{
    /// <summary>
    /// Gets the CPU that trapped.
    /// </summary>
    public ICpu Cpu { get; }

    /// <summary>
    /// Gets the trap code.
    /// </summary>
    public ulong TrapCode { get; }

    /// <summary>
    /// Get a value indicating whether the trap is a syscall.
    /// </summary>
    public bool IsSyscall { get; }

    /// <summary>
    /// Gets the syscall id.
    /// </summary>
    public ulong SyscallId { get; }

    /// <summary>
    /// Gets the value of the first argument register.
    /// </summary>
    public ulong Argument0 { get; }

    /// <summary>
    /// Gets the value of the second argument register.
    /// </summary>
    public ulong Argument1 { get; }

    /// <summary>
    /// Gets the value of the third argument register.
    /// </summary>
    public ulong Argument2 { get; }

    /// <summary>
    /// Gets the value of the first float argument register.
    /// </summary>
    public float FloatArgument0 { get; }

    /// <summary>
    /// Gets the value of the first double argument register.
    /// </summary>
    public double DoubleArgument0 { get; }

    /// <summary>
    /// Gets or sets the value of the first return value register.
    /// </summary>
    public ulong Result0 { get; set; }

    /// <summary>
    /// Gets or sets the value of the second return value register.
    /// </summary>
    public ulong Result1 { get; set; }

    /// <summary>
    /// Gets or sets the value of the first float return value register.
    /// </summary>
    public float FloatResult0 { get; set; }

    /// <summary>
    /// Gets or sets the value of the first double return value register.
    /// </summary>
    public double DoubleResult0 { get; set; }
}
