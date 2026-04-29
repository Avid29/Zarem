// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Devices;
using Zarem.Models.Instructions.Enums;
using Zarem.N64.Devices;

namespace Zarem.N64;

/// <summary>
/// An N64 <see cref="ComputerBase"/>.
/// </summary>
public class Nintendo64 : MipsComputer
{
#pragma warning disable CS1591
    public const ulong MemoryBase = 0x00000000;
    public const ulong MemorySize = 0x00400000;
    public const ulong RcpBase = 0x04000000;
    public const ulong RcpSize = 0x01000000;
    public const ulong DmaInterfaceBase = 0x04600000;
    public const ulong DmaInterfaceSize = 0x00000020;
    public const ulong CartridgeBase = 0x1000_0000;
    public const ulong CartridgeSize = 0x1FC0_0000;
    public const ulong RomChipBase = 0x1FC00000;
    public const ulong RomChipSize = 0x00000800;
#pragma warning restore CS1591

    private readonly N64CartridgeSlot _cartridgeSlot;

    /// <summary>
    /// Initializes a new instance of the <see cref="Nintendo64"/> class.
    /// </summary>
    public Nintendo64(ExecutionMode mode) : base(new MipsEmulatorConfig(MipsVersion.MipsIII, mode))
    {
        _cartridgeSlot = new N64CartridgeSlot(CartridgeSize);
    }

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
        mapper.MapDevice(RcpBase, new RealityCoProcessor(Memory.Physical));
        mapper.MapDevice(CartridgeBase, _cartridgeSlot);
    }
}
