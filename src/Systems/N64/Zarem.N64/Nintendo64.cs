// Avishai Dernis 2026

using Zarem.Emulator.Devices;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Machine;
using Zarem.N64.Config;
using Zarem.N64.Devices;
using Zarem.N64.Devices.RealityCoProcessor;

namespace Zarem.N64;

/// <summary>
/// An N64 <see cref="ComputerBase"/>.
/// </summary>
public class Nintendo64 : MipsComputer
{
#pragma warning disable CS1591
    public const ulong MemoryBase = 0x0000_0000;
    public const ulong MemorySize = 0x0040_0000;
#pragma warning restore CS1591

    private readonly N64CartridgeSlot _cartridgeSlot;

    /// <summary>
    /// Initializes a new instance of the <see cref="Nintendo64"/> class.
    /// </summary>
    public Nintendo64(N64EmulatorConfig config) : base(config, false)
    {
        _cartridgeSlot = new N64CartridgeSlot();
        RealityCoProcessor = new RealityCoProcessor(Memory.Physical);

        RemapDevices();
    }

    /// <summary>
    /// Gets the Reality Co-Processor (RCP) device.
    /// </summary>
    public RealityCoProcessor RealityCoProcessor { get; }

    /// <summary>
    /// Insert a z64 as a cartridge.
    /// </summary>
    public void InsertCartridge(string z64FilePath)
    {
        _cartridgeSlot.LoadCartridge(z64FilePath);
    }

    /// <summary>
    /// Ejects the current cartridge in the slot.
    /// </summary>
    public void EjectCartridge()
    {
        _cartridgeSlot.UnloadCartridge();
    }

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        mapper.MapDevice(MemoryBase, new RamDevice(MemorySize));
        mapper.MapDevice(RealityCoProcessor.BusRangeBase, RealityCoProcessor);
        mapper.MapDevice(N64CartridgeSlot.MappingBase, _cartridgeSlot);
    }
}
