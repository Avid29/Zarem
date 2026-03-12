// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Models.Files;

namespace Zarem.Models.Breakpoints;

/// <summary>
/// A collection of breakpoints in a <see cref=""/>
/// </summary>
public class BreakpointCollection
{
    private readonly HashSet<BreakpointIdentity> _breakpoints = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BreakpointCollection"/> class.
    /// </summary>
    /// <param name="file"></param>
    public BreakpointCollection(SourceFile file)
    {
        File = file;
    }

    /// <summary>
    /// Gets the source file the breakpoints belong to.
    /// </summary>
    public SourceFile File { get; }

    /// <summary>
    /// Gets the breakpoint source for update to date line information.
    /// </summary>
    public IBreakpointSource? Source { get; set; }

    /// <summary>
    /// Gets the hashset of breakpoints in the collection.
    /// </summary>
    public IEnumerable<BreakpointIdentity> Breakpoints => _breakpoints;

    /// <summary>
    /// Adds a new breakpoint at the given line.
    /// </summary>
    /// <param name="line">The line to add the breakpoint.</param>
    public BreakpointIdentity Add(ulong line)
    {
        var breakpoint = new BreakpointIdentity(this, line);
        _breakpoints.Add(breakpoint);
        return breakpoint;
    }

    /// <summary>
    /// Removes a breakpoint from the collection.
    /// </summary>
    /// <param name="id">The breakpoint to remove</param>
    public void Remove(BreakpointIdentity id)
    {
        _breakpoints.Remove(id);
    }
}
