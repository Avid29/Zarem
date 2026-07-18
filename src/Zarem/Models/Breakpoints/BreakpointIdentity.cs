// Avishai Dernis 2026

using System;

namespace Zarem.Models.Breakpoints;

/// <summary>
/// An identity for a breakpoint in a 
/// </summary>
public class BreakpointIdentity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BreakpointIdentity"/> class
    /// </summary>
    public BreakpointIdentity(BreakpointCollection parent, ulong line)
    {
        Parent = parent;
        Line = line;
    }

    /// <summary>
    /// Gets the breakpoint identity.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the parent <see cref="BreakpointCollection"/>.
    /// </summary>
    public BreakpointCollection Parent { get; }

    /// <summary>
    /// Gets the line the breakpoint occurs on.
    /// </summary>
    public ulong Line
    {
        get
        {
            // Get the most up-to-date line if a source is attached
            if (Parent.Source is not null)
            {
                var liveLine = Parent.Source.GetBreakpointLine(this);
                if (liveLine.HasValue)
                {
                    field = liveLine.Value;
                }
            }

            return field;
        }
        set => field = value;
    }
}
