// Avishai Dernis 2026

using System;
using System.Buffers;
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
    /// <param name="encoding">The encoding to use when reading the string.</param>
    /// <param name="maxBytes">The max size of the string to read.</param>
    /// <returns>The string located at that address.</returns>
    public static string ReadString(this IMemoryAccessor memory, uint address, Encoding encoding, int maxBytes = 0)
    {
        // Determine null-terminator width (stride)
        int stride = encoding.GetByteCount("\0");
        Span<byte> unitBuffer = stackalloc byte[stride];

        // Determine buffer size (respect maxBytes if provided)
        int initialSize = maxBytes > 0 ? Math.Min(maxBytes, 1024) : 256;
        byte[] rentedArray = ArrayPool<byte>.Shared.Rent(initialSize);
        int totalBytesRead = 0;

        while (true)
        {
            // Stop if we cannot fit another full 'unit' (stride) within maxBytes
            if (maxBytes > 0 && (totalBytesRead + stride) > maxBytes)
                break;

            bool isNull = true;
            for (int i = 0; i < stride; i++)
            {
                byte b = memory.Read<byte>(address + (uint)i);
                unitBuffer[i] = b;
                if (b != 0)
                {
                    isNull = false;
                }
            }

            // Stop on null terminator
            if (isNull)
                break;

            // Resize buffer if we're not using maxBytes or if we need more space
            if (totalBytesRead + stride > rentedArray.Length)
            {
                int nextSize = maxBytes > 0 ? maxBytes : rentedArray.Length * 2;
                byte[] newArray = ArrayPool<byte>.Shared.Rent(nextSize);
                Buffer.BlockCopy(rentedArray, 0, newArray, 0, totalBytesRead);
                ArrayPool<byte>.Shared.Return(rentedArray);
                rentedArray = newArray;
            }

            // Copy unit into the main buffer
            unitBuffer.CopyTo(rentedArray.AsSpan(totalBytesRead));
            totalBytesRead += stride;
            address += (uint)stride;
        }

        // Decode the string, release the pooled array, and return the string
        var str = encoding.GetString(rentedArray.AsSpan(0, totalBytesRead));
        ArrayPool<byte>.Shared.Return(rentedArray);
        return str;
    }
}
