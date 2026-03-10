// Avishai Dernis 2025

namespace Zarem.Models.Tables;

/// <summary>
/// A struct describing a location in a source file.
/// </summary>
public readonly struct SourceLocation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceLocation"/> struct.
    /// </summary>
    public SourceLocation(string? file = null)
    {
        File = file;
        Index = 0;
        Line = 1;
        Column = 1;
    }

    /// <summary>
    /// Gets the source file the location is in.
    /// </summary>
    public string? File { get; init; }

    /// <summary>
    /// Gets the index of the location.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the row of the location file.
    /// </summary>
    /// <remarks>
    /// The line is 1 indexed.
    /// </remarks>
    public int Line { get; init; }

    /// <summary>
    /// Gets the column of the location.
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Gets the next line
    /// </summary>
    /// <returns></returns>
    public SourceLocation NextLine(int incSize = 1)
        => new()
        {
            File = File,
            Index = Index + incSize,
            Line = Line + 1,
            Column = 1,
        };

    /// <summary>
    /// Adds a number of characters to the location.
    /// </summary>
    public static SourceLocation operator +(SourceLocation pos, int inc)
        => new()
        {
            File = pos.File,
            Index = pos.Index + inc,
            Line = pos.Line,
            Column = pos.Column + inc,
        };
}
