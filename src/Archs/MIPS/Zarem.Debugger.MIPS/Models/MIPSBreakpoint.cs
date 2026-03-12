// Avishai Dernis 2026

using Zarem.Debugger.Models;
using Zarem.Models.Instructions;

namespace Zarem.Debugger.MIPS.Models;

/// <summary>
/// A breakpoint in a MIPS executable.
/// </summary>
public class MipsBreakpoint : Breakpoint
{
    /// <summary>
    /// Gets or sets the instruction to break in-place of.
    /// </summary>
    /// <remarks>
    /// This instruction is replaced with a break instruction in the code.
    /// It must be executed after the breakpoint is hit.
    /// </remarks>
    public MipsInstruction OriginalInstruction { get; private set; }

    /// <inheritdoc/>
    public override bool TryDisable()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool TryEnable()
    {
        throw new NotImplementedException();
    }
}
