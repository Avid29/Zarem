// Avishai Dernis 2025

namespace Zarem.Models.Tables;

/// <summary>
/// A struct describing a range of a location in a source file.
/// </summary>
public readonly struct SourceRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceRange"/> struct.
    /// </summary>
    public SourceRange(SourceLocation start, long size)
    {
        Start = start;
        Size = size;
    }

    /// <summary>
    /// Gets the starting position of the source range.
    /// </summary>
    public SourceLocation Start { get; }

    /// <summary>
    /// Gets the size of the range.
    /// </summary>
    public long Size { get; }
}
