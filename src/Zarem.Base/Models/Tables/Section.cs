// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.IO;

namespace Zarem.Models.Tables;

/// <summary>
/// A section in the <see cref="Module"/>.
/// </summary>
public class Section
{
    private readonly List<RelocationEntry> _relocations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Section"/> class.
    /// </summary>
    public Section(string name, uint alignment = 1, Stream? stream = null)
    {
        Name = name;
        Alignment = alignment;
        Stream = stream ?? new MemoryStream();
    } 

    /// <summary>
    /// Gets the section's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the section's alignment.
    /// </summary>
    public uint Alignment { get; }

    /// <summary>
    /// Gets the section's stream.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// Gets or sets the virtual address of the section when loaded.
    /// </summary>
    public ulong VirtualAddress { get; set; }

    /// <summary>
    /// Gets the active position within the section.
    /// </summary>
    public long Position
    {
        get => Stream.Position;
        set => Stream.Position = value;
    }

    /// <summary>
    /// Gets the size of the section.
    /// </summary>
    public long Size => Stream.Length;

    /// <summary>
    /// Gets the current address within the section.
    /// </summary>
    public Address CurrentAddress => new(this, Stream.Position);

    /// <summary>
    /// Gets a list of the relocations in the section.
    /// </summary>
    public IReadOnlyList<RelocationEntry> Relocations => _relocations;

    /// <summary>
    /// Appends an array of bytes to the end of the section.
    /// </summary>
    /// <remarks>
    /// Bytes must be in architecture-appropriate endianness.
    /// </remarks>
    public void Append(ReadOnlySpan<byte> bytes) => Stream.Write(bytes);

    /// <summary>
    /// Appends a stream of data to the end of the section
    /// </summary>
    /// <param name="stream">The stream to append.</param>
    /// <param name="seek">Whether or not seek to the end before appending.</param>
    public void Append(Stream stream, bool seek = true)
    {
        if (seek)
        {
            // Seek streams
            stream.Seek(0, SeekOrigin.Begin);   // Read from the front of source
            Stream.Seek(0, SeekOrigin.End);     // Write to the back of destination
        }

        stream.CopyTo(Stream);
    }

    /// <summary>
    /// Reserves a number of bytes in the section.
    /// </summary>
    public void Reserve(int size) => WriteZeroes(size);

    /// <summary>
    /// Aligns the 
    /// </summary>
    /// <param name="boundary"></param>
    public void Align(uint boundary)
    {
        if (boundary <= 1)
            return;

        long padding = (boundary - (Stream.Position % boundary)) % boundary;
        if (padding > 0)
            WriteZeroes(padding);
    }

    private void WriteZeroes(long count)
    {
        Span<byte> zero = stackalloc byte[64];
        while (count > 0)
        {
            int chunk = (int)Math.Min(count, zero.Length);
            Stream.Write(zero[..chunk]);
            count -= chunk;
        }
    }

    /// <summary>
    /// Tracks a new relocation in the section.
    /// </summary>
    public void AddRelocation(RelocationEntry relocation) => _relocations.Add(relocation);
}
