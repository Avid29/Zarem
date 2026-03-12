// Avishai Dernis 2026

namespace Zarem.Debugger.Models;

/// <summary>
/// A breakpoint in the code.
/// </summary>
public class Breakpoint
{
    /// <summary>
    /// Initiailizes a new instance of the <see cref="Breakpoint"/> class.
    /// </summary>
    public Breakpoint(ulong address, int swapSize)
    {
        Address = address;
        Swap = new byte[swapSize];
    }

    /// <summary>
    /// Gets or sets the breakpoint address.
    /// </summary>
    public ulong Address { get; }

    /// <summary>
    /// Gets or sets the bytes swapped out to insert the breakpoint.
    /// </summary>
    public byte[] Swap { get; internal set; }

    /// <summary>
    /// Gets whether or not the breakpoint is applied.
    /// </summary>
    public bool IsApplied { get; internal set; }
}
