// Avishai Dernis 2026

namespace Zarem.Debugger.Models;

/// <summary>
/// A class representing a register, containing metadata on its properties.
/// </summary>
public class RegisterMeta
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterMeta"/> class.
    /// </summary>
    public RegisterMeta(string name, string? category = null, int size = 32, bool floatingPoint = false)
    {
        Name = name;
        Category = category;
        Size = size;
        FloatingPoint = floatingPoint;
    }

    /// <summary>
    /// Gets the register's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the register's category name.
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// Gets the register's size in bits.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets whether or not the register is a floating point register.
    /// </summary>
    public bool FloatingPoint { get; }
}
