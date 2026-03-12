// Avishai Dernis 2026

namespace Zarem.Models.Breakpoints;

/// <summary>
/// An interface for a breakpoint source which can dynamically adjust the line they belong to.
/// </summary>
public interface IBreakpointSource
{
    /// <summary>
    /// Gets the updated line of a <see cref="BreakpointIdentity"/>.
    /// </summary>
    /// <param name="id">The <see cref="BreakpointIdentity"/> to get the line of.</param>
    /// <returns>The line the breakpoint occurs on, if any/</returns>
    public ulong? GetBreakpointLine(BreakpointIdentity id);
}
