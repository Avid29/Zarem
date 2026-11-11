// Avishai Dernis 2026

using System;
using Zarem.Emulator.Devices;

namespace Zarem.N64.Devices;

/// <summary>
/// A <see cref="RamDevice"/> implementation for the <see cref="Nintendo64"/>.
/// </summary>
public class RdramDevice : RamDevice
{
    private const ulong BaseMemorySize = 4 * 1024 * 1024;
    private const ulong ExpansionPakMemorySize = 4 * 1024 * 1024;
    private const ulong ExpandedMemorySize = BaseMemorySize + ExpansionPakMemorySize;
    private const ulong DomainMemorySize = 0x0400_0000;

    private readonly uint _mirrorMask;

    /// <summary>
    /// Initializes a new instance of the <see cref="RdramDevice"/> class.
    /// </summary>
    public RdramDevice(bool hasExpansionPak, uint pageSize = 4096) : base(hasExpansionPak ? ExpandedMemorySize : BaseMemorySize, pageSize)
    {
        // Mask for wrapping addresses. E.g., for 8MB (0x800000), mask is 0x7FFFFF.
        // Any address >= 8MB will wrap safely back into the buffer via bitwise AND.
        var size = hasExpansionPak ? ExpandedMemorySize : BaseMemorySize;
        _mirrorMask = (uint)(size - 1);
    }

    /// <inheritdoc/>
    public override string Name => "RDRAM";

    /// <inheritdoc/>
    public override ulong BusRangeSize => DomainMemorySize;

    /// <inheritdoc/>
    public override void Read(ulong offset, Span<byte> destination)
        => base.Read(offset & _mirrorMask, destination);

    /// <inheritdoc/>
    public override void Write(ulong offset, ReadOnlySpan<byte> source)
        => base.Write(offset & _mirrorMask, source);

    /// <inheritdoc/>
    public override unsafe byte* GetPointer(ulong offset)
        => base.GetPointer(offset & _mirrorMask);
}
