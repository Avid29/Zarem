// Adam Dernis 2024


// Adam Dernis 2024

using Zarem.Assembler.Models.Directives.Abstract;

namespace Zarem.Assembler.Models.Directives;

/// <summary>
/// A <see cref="Directive"/> for memory alignments.
/// </summary>
public class AlignDirective(uint boundary) : Directive
{
    /// <summary>
    /// Gets the alignment boundary.
    /// </summary>
    public uint Boundary { get; } = boundary;
}
