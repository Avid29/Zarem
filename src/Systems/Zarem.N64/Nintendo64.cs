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
    private const ulong CartridgeAddres = 0x10000000;

    /// <summary>
    /// Initializes a new instance of the <see cref="Nintendo64"/> class.
    /// </summary>
    public Nintendo64() : base(new MipsEmulatorConfig(MipsVersion.MipsIII))
    {
    }

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        // Map 4MB of RAM
        mapper.MapDevice(0x0, new RamDevice(4 * 1024 * 1024));

        // Map the game cartridge
        mapper.MapDevice(CartridgeAddres, new N64Cartridge(string.Empty));
    }
}
