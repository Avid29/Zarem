// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Models.Tables;

namespace Zarem.Helpers;

/// <summary>
/// A class for converting <see cref="SourceLocation"/> and <see cref="SourceRange"/> models between encodings.
/// </summary>
public class LocationMapper
{
    private readonly SortedList<long, long> _mapping = [];

    /// <summary>
    /// Registers a point of reference where the mapping between encodings is known.
    /// </summary>
    /// <param name="sourceIndex">The index in the source encoding.</param>
    /// <param name="targetIndex">The index in the target encoding.</param>
    public void RegisterReferencePoint(long sourceIndex, long targetIndex)
    {
        _mapping[sourceIndex] = targetIndex;
    }

    /// <summary>
    /// Clears the mapping indicies.
    /// </summary>
    public void Clear() => _mapping.Clear();

    /// <summary>
    /// Translates a <see cref="SourceLocation"/> to the target encoding using linear interpolation.
    /// </summary>
    public SourceLocation Translate(SourceLocation source)
    {
        if (_mapping.Count is 0)
            return source;

        long sourceIdx = source.Index;
        long translatedIdx = CalculateMappedIndex(sourceIdx);

        return new SourceLocation(source.File)
        {
            Index = translatedIdx,
            Line = source.Line,   // Assuming line/column logic is handled by the consumer 
            Column = source.Column // or stays consistent relative to the index change.
        };
    }

    /// <summary>
    /// Translates a <see cref="SourceRange"/> to the target encoding.
    /// </summary>
    public SourceRange Translate(SourceRange range)
    {
        if (_mapping.Count is 0)
            return range;

        var newStart = Translate(range.Start);

        // Calculate the end point in target encoding to determine the new Size
        long sourceEndIdx = range.Start.Index + range.Size;
        long targetEndIdx = CalculateMappedIndex(sourceEndIdx);

        return new SourceRange(newStart, targetEndIdx - newStart.Index);
    }

    private long CalculateMappedIndex(long sourceIdx)
    {
        // Exact match or single point
        if (_mapping.TryGetValue(sourceIdx, out long exactMatch))
            return exactMatch;

        // Find bounds for interpolation
        int index = BinarySearchKeys(sourceIdx);

        // If outside the range of registered points, we apply the offset of the nearest bound
        if (index == -1)
        {
            return sourceIdx + (_mapping.Values[0] - _mapping.Keys[0]);
        }
        if (index >= _mapping.Count - 1)
        {
            return sourceIdx + (_mapping.Values[^1] - _mapping.Keys[^1]);
        }

        // Linear Interpolation: y = y0 + (x - x0) * (y1 - y0) / (x1 - x0)
        long x0 = _mapping.Keys[index];
        long x1 = _mapping.Keys[index + 1];
        long y0 = _mapping.Values[index];
        long y1 = _mapping.Values[index + 1];

        double ratio = (double)(sourceIdx - x0) / (x1 - x0);
        return y0 + (long)Math.Round(ratio * (y1 - y0));
    }

    private int BinarySearchKeys(long key)
    {
        int low = 0;
        int high = _mapping.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (_mapping.Keys[mid] == key) return mid;
            if (_mapping.Keys[mid] < key) low = mid + 1;
            else high = mid - 1;
        }
        return high; // Returns the index of the greatest key less than the search key
    }
}
