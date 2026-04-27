// Avishai Dernis 2026

using Zarem.Emulator.Config;
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
    private const ulong MemorySize = 0x0040_0000;           // 4MB of RAM
    private const ulong CartridgeAddress = 0x1000_0000;     // Cartridge address
    private const ulong CartridgeRangeSize = 0x1000_0000;   // The size of the cartridge range

    private readonly N64CartridgeSlot _cartridgeSlot;

    /// <summary>
    /// Initializes a new instance of the <see cref="Nintendo64"/> class.
    /// </summary>
    public Nintendo64() : base(new MipsEmulatorConfig(MipsVersion.MipsIII))
    {
        _cartridgeSlot = new N64CartridgeSlot(CartridgeRangeSize);
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
        mapper.MapDevice(0x0, new RamDevice(MemorySize));

        // Map the game cartridge
        mapper.MapDevice(CartridgeAddress, _cartridgeSlot);
    }
}
