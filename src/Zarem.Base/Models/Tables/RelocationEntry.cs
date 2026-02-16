// Avishai Dernis 2026

namespace Zarem.Models.Tables;

/// <summary>
/// A relocation entry.
/// </summary>
public sealed class RelocationEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelocationEntry"/> class.
    /// </summary>
    public RelocationEntry(string symbolName, Address location, uint type, long addend = 0)
    {
        SymbolName = symbolName;
        Location = location;
        Type = type;
        Addend = addend;
    }

    /// <summary>
    /// Gets the name of the symbol reference being relocated.
    /// </summary>
    public string SymbolName { get; }

    /// <summary>
    /// Gets the relocation's address.
    /// </summary>
    public Address Location { get; set; }

    /// <summary>
    /// Gets the type of relocation.
    /// </summary>
    /// <remarks>
    /// Handled differently depending on architecture.
    /// </remarks>
    public uint Type { get; }

    /// <summary>
    /// Gets the addend component
    /// </summary>
    public long Addend { get; }
}
