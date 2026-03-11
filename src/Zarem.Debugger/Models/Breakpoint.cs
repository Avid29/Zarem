// Avishai Dernis 2026

using Zarem.Models;

namespace Zarem.Debugger.Models;

/// <summary>
/// A breakpoint in the code.
/// </summary>
public abstract class Breakpoint
{
    /// <summary>
    /// Gets or sets the breakpoint address.
    /// </summary>
    public Address Address { get; }

    /// <summary>
    /// Gets or sets whether or not the breakpoint is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Attempts to enable a breakpoint, inserting a break instruction.
    /// </summary>
    /// <returns>Whether or not the breakpoint was successfuly enabled.</returns>
    public abstract bool TryEnable();

    /// <summary>
    /// Attempts to disable a breakpoint, reverting the inserted break instruction.
    /// </summary>
    /// <returns>Whether or not the breakpoint was successfuly disabled.</returns>
    public abstract bool TryDisable();
}
