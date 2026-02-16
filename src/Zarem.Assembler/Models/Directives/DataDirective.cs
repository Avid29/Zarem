// Adam Dernis 2024

using Zarem.Assembler.Models.Directives.Abstract;

namespace Zarem.Assembler.Models.Directives;

/// <summary>
/// A <see cref="Directive"/> for data allocations.
/// </summary>
public class DataDirective(byte[] data) : Directive
{
    /// <summary>
    /// Gets the data allocated by the directive.
    /// </summary>
    public byte[] Data { get; } = data;
}
