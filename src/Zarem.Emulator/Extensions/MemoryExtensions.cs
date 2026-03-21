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
        int stride = encoding.GetByteCount("\0");
        Span<byte> unitBuffer = stackalloc byte[stride];

        int initialSize = maxBytes > 0 ? Math.Min(maxBytes, 1024) : 256;
        byte[] rentedArray = ArrayPool<byte>.Shared.Rent(initialSize);
        int totalBytesRead = 0;

        try
        {
            while (true)
            {
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
                {
                    break;
                }

                // Grow the array if needed
                if (totalBytesRead + stride > rentedArray.Length)
                {
                    int nextSize = maxBytes > 0 ? maxBytes : rentedArray.Length * 2;
                    byte[] newArray = ArrayPool<byte>.Shared.Rent(nextSize);
                    Buffer.BlockCopy(rentedArray, 0, newArray, 0, totalBytesRead);
                    ArrayPool<byte>.Shared.Return(rentedArray);
                    rentedArray = newArray;
                }

                // Copy unitBuffer into the main buffer
                unitBuffer.CopyTo(rentedArray.AsSpan(totalBytesRead));
                totalBytesRead += stride;
                address += (uint)stride;
            }

            // Decode and return the string
            return encoding.GetString(rentedArray.AsSpan(0, totalBytesRead));
        }
        finally
        {
            // Always release the pooled array
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}
