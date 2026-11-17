// Avishai Dernis 2026

using System;
using System.Buffers;
using Zarem.Emulator.Machine.Memory;

namespace Zarem.N64.Devices.RealityCoProcessor.Abstract;

/// <summary>
/// A base class for <see cref="IRealityCoProcessorDevice"/> classes with DMA features.
/// </summary>
public abstract class N64DmaDeviceBase : IRealityCoProcessorDevice
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64DmaDeviceBase"/> class.
    /// </summary>
    public N64DmaDeviceBase(PhysicalBus bus)
    {
        Bus = bus;
    }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract ulong BusRangeBase { get; }

    /// <inheritdoc/>
    public abstract ulong BusRangeSize { get; }

    /// <summary>
    /// Gets the <see cref="PhysicalBus"/> the device is connected to.
    /// </summary>
    protected PhysicalBus Bus { get; }

    /// <inheritdoc/>
    public abstract void Read(ulong offset, Span<byte> destination);

    /// <inheritdoc/>
    public abstract void Write(ulong offset, ReadOnlySpan<byte> source);

    /// <inheritdoc/>
    public abstract void Dispose();

    /// <summary>
    /// Executes a DMA transfer
    /// </summary>
    /// <param name="dramAddress">The starting address in DRAM.</param>
    /// <param name="deviceAddress">The starting address in the device.</param>
    /// <param name="transferLength">The length of a transfer block.</param>
    /// <param name="writeToDram">Specific read/write from DRAM</param>
    /// <param name="count">The number of blocks to transfer.</param>
    /// <param name="skip">The amount to skip within the device after a block.</param>
    protected void ExecuteDmaTransfer(uint dramAddress, uint deviceAddress, uint transferLength, bool writeToDram, uint count = 1, uint skip = 0)
    {
        // Mark busy
        SetBusyState(true);

        // Optimization: Merge needlessly batched transfers
        if (count is not 1 && skip is 0)
        {
            transferLength *= count;
            count = 1;
        }

        // Allocate copy buffer
        byte[]? pooledArray = null;
        var span = transferLength <= 1024 ?
            stackalloc byte[(int)transferLength] :
            (pooledArray = ArrayPool<byte>.Shared.Rent((int)transferLength)).AsSpan(0, (int)transferLength);

        // TODO - Optimization: Replace bus calls with direct use of devices and RDRAM instance
        // NOTE: When implemeneted, an event will still need to be raised that the address was written
        // in order to properly block swaps in JIT conditions.

        for (uint i = 0; i < count; i++)
        {
            ulong currentDeviceAddress = deviceAddress + (i * transferLength) + (i * skip);
            ulong currentDramAddress = dramAddress + (i * transferLength);

            if (writeToDram)
            {
                // Source -> RDRAM
                Bus.Read(currentDeviceAddress, span);
                Bus.Write(currentDramAddress, span);
            }
            else
            {
                // RDRAM -> Destination
                Bus.Read(currentDramAddress, span);
                Bus.Write(currentDeviceAddress, span);
            }
        }

        // Free temporary array
        if (pooledArray is not null)
        {
            ArrayPool<byte>.Shared.Return(pooledArray);
        }

        // Clear busy
        SetBusyState(false);
    }

    /// <summary>
    /// Mark the device as busy or done.
    /// </summary>
    protected abstract void SetBusyState(bool busy);
}
