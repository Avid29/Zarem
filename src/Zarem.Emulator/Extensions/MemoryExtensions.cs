// Avishai Dernis 2026

using System.Text;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Extensions;

/// <summary>
/// A class containing static extension methods for memory related interfaces.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Reads a string from memory.
    /// </summary>
    /// <param name="memory">The memory accessor to read from.</param>
    /// <param name="address">The address to read from.</param>
    /// <returns>The string located at that address.</returns>
    public static string ReadString(this IMemoryAccessor memory, uint address)
    {
        // Construct the string through reading memory
        StringBuilder builder = new();
        var c = memory.Read<byte>(address);
        while (c is not 0)
        {
            address++;
            builder.Append((char)c);
            c = memory.Read<byte>(address);
        }

        return $"{builder}";
    }
}
