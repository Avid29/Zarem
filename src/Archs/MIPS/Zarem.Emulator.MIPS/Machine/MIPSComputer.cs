// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Devices;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the MIPS interpreter.
/// </summary>
public class MipsComputer : ComputerBase
{
    private readonly MemoryMapper _memoryMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsComputer"/> class.
    /// </summary>
    public MipsComputer(MIPSEmulatorConfig config)
    {
        Config = config;

        // Create the physical memory bus
        _memoryMapper = new MemoryMapper();
        var bus = new PhysicalBus(_memoryMapper);
        MapDevices(_memoryMapper);

        // Initialize the components
        Processor = new MipsCpu(config, bus);
        Memory = new MemorySystem(bus, Processor.Tlb);

        // Hook the virtual memory system into the Cpu
        Processor.Memory = Memory;

        Processor.ShutdownRequested += Processor_ShutdownRequested;
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
    public override IEnumerable<IDevice> Devices => _memoryMapper.Devices;

    /// <inheritdoc/>
    public override void Tick()
    {
        Processor.Step();
    }

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        // System RAM
        mapper.MapDevice(0x0000_0000, new RamDevice(1024 * 1024 * 1024)); // TODO: Config ram size

        // Graphics Buffer 
        //mapper.MapDevice(0x1300_0000, new ZaremGBU());
    }

    private void Processor_ShutdownRequested(object? sender, EventArgs e)
    {
        Processor.ShutdownRequested -= Processor_ShutdownRequested;
        RequestShutdown();
    }
}
