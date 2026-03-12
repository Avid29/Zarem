// Avishai Dernis 2025

using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Devices;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the MIPS interpreter.
/// </summary>
public class MipsComputer : ComputerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsComputer"/> class.
    /// </summary>
    public MipsComputer(MIPSEmulatorConfig config)
    {
        Config = config;

        // Create the physical memory bus
        var mapper = new MemoryMapper();
        var bus = new PhysicalBus(mapper);
        MapDevices(mapper);

        // Initialize the components
        Processor = new MipsCpu(config, bus);
        Memory = new MemorySystem(bus, Processor.Tlb);

        // Hook the virtual memory system into the Cpu
        Processor.Memory = Memory;
    }

    /// <summary>
    /// Gets the processor of the computer system.
    /// </summary>
    public MipsCpu Processor { get; }

    /// <inheritdoc/>
    public override ICpu Cpu => Processor;

    /// <summary>
    /// Gets the emulation configuration to follow for computing.
    /// </summary>
    public MIPSEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override IMemorySystem Memory { get; }

    /// <inheritdoc/>
    public override void Tick()
    {
        Processor.Step();
    }

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        mapper.MapDevice(0x0, new RamDevice(1024 * 1024 * 1024)); // TODO: Config ram size
    }
}
