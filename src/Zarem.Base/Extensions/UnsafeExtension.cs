// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;

namespace Zarem.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="Unsafe"/> class, providing additional unsafe operations for memory manipulation.
/// </summary>
public unsafe static class UnsafeExtension
{
    extension(Unsafe)
    {
        /// <summary>
        /// Copies a block of memory from the source pointer to the destination span.
        /// </summary>
        public static void CopyBlock(Span<byte> destination, byte* source)
        {
            fixed(byte* destPtr = destination)
            {
                Unsafe.CopyBlock(destPtr, source, (uint)destination.Length);
            }
        }

        /// <summary>
        /// Copies a block of memory from the source pointer to the destination span.
        /// </summary>
        public static void CopyBlock(byte* destination, ReadOnlySpan<byte> source)
        {
            fixed(byte* srcPtr = source)
            {
                Unsafe.CopyBlock(destination, srcPtr, (uint)source.Length);
            }
        }
    }
}
