// Avishai Dernis 2026

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Machine.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor.Abstract;

/// <summary>
/// A base class for <see cref="IRealityCoProcessorDevice"/> classes with register features.
/// </summary>
public abstract class N64RegisterDeviceBase<T> : IRealityCoProcessorDevice
    where T : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64RegisterDeviceBase{T}"/> class.
    /// </summary>
    public N64RegisterDeviceBase(FlatMemoryStore regs)
    {
        Registers = regs;
    }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract ulong BusRangeBase { get; }

    /// <inheritdoc/>
    public abstract ulong BusRangeSize { get; }

    /// <summary>
    /// Gets the <see cref="FlatMemoryStore"/> used to represent the device's MMIO registers.
    /// </summary>
    protected FlatMemoryStore Registers { get; }

    /// <inheritdoc/>
    public virtual void Read(ulong offset, Span<byte> destination) => Registers.Read(offset, destination);

    /// <inheritdoc/>
    public virtual void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        // Get register write behavior info
        var register = Unsafe.BitCast<ulong, T>(offset);
        bool hasSideEffects = HasWriteSideEffects(register, out bool directWrite);

        if (directWrite)
        {
            Registers.Write(offset, source);
        }
        
        if (hasSideEffects)
        {
            uint value = BinaryPrimitives.ReadUInt32BigEndian(source);
            HandleWriteSideEffects(register, value);
        }
    }

    /// <summary>
    /// Checks whether or not the register has side effects on write.
    /// </summary>
    /// <param name="register">The register being written to.</param>
    /// <param name="directWrite">Whether or not to write to the register directly (before performing side-effects).</param>
    /// <returns>Whether or not the register has side effects.</returns>
    protected virtual bool HasWriteSideEffects(T register, out bool directWrite)
    {
        directWrite = true;
        return false;
    }

    /// <summary>
    /// Handles the side effects of writing to a register.
    /// </summary>
    /// <param name="register">The register written to.</param>
    /// <param name="value">The value written to the register.</param>
    protected virtual void HandleWriteSideEffects(T register, uint value)
    {
    }

    /// <inheritdoc/>
    public void Dispose() => Registers.Dispose();
}
