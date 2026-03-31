// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Parsers.Enums;

namespace Zarem.Assembler.Extensions;

/// <summary>
/// A class containing extension methods for integer types.
/// </summary>
public static class IntegerExtensions
{
    extension(long value)
    {
        /// <summary>
        /// Casts a value to a specified bit count and shift amount, while also checking for any changes that occured during the cast.
        /// </summary>
        /// <remarks>
        /// This does not apply the <paramref name="shiftAmount"/>! It only masks the lower bits.
        /// </remarks>
        /// <param name="integer">A reference to the integer to modify.</param>
        /// <param name="bitCount">The number of bits after casting.</param>
        /// <param name="shiftAmount">The number of bits that will drop from the bottom.</param>
        /// <param name="signed">Whether or not the new value should be signed.</param>
        /// <param name="changes">The changes made to the integer.</param>
        /// <returns>Whether or not the value can be safely cast.</returns>
        public static bool TryCast(ref long integer, int bitCount, int shiftAmount, bool signed, out CastingChanges changes)
        {
            var original = integer;

            Guard.IsGreaterThan(bitCount, 1);
            Guard.IsLessThanOrEqualTo(bitCount + shiftAmount, 64);

            // Create a masks for the high and low truncating bits,
            // as well as an overall remaining bits map
            var upperMask = bitCount == 64 ? -1L : (1L << (bitCount + shiftAmount)) - 1;
            var lowerMask = ~((1L << shiftAmount) - 1);
            var mask = upperMask & lowerMask;

            // Truncate mask upper and lower bits
            long truncated = integer & mask;

            // Sign extend if signed and not full width
            if (signed && bitCount < 64)
            {
                long signBit = 1L << (bitCount - 1);
                if ((truncated & signBit) != 0)
                    truncated |= ~upperMask; // Sign extend
            }

            integer = truncated;

            // Compute changes
            changes = CastingChanges.None;

            // Check if the sign was dropped
            if (!signed && original < 0)
                changes |= CastingChanges.SignChanged;

            // Check for upper truncation
            long upperBits = original & ~upperMask;
            if (upperBits != 0 && upperBits != ~upperMask)
                changes |= CastingChanges.TruncatedHigh;

            // Check for lower truncation
            if ((original & ~lowerMask) != 0)
            {
                changes |= CastingChanges.TruncatedLow;
            }

            // Return combined code
            return changes is CastingChanges.None;
        }
    }
}
