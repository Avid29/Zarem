// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.Devices;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.Extensions;
using Zarem.Models.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the MIPS interpreter.
/// </summary>
public sealed class MipsComputer : ComputerBase
{
    private readonly MemoryMapper _memoryMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsComputer"/> class.
    /// </summary>
    public MipsComputer(MipsEmulatorConfig config)
    {
        Config = config;

        // Create the physical memory bus
        _memoryMapper = new MemoryMapper();
        var bus = new PhysicalBus(_memoryMapper, Endianness.Big);
        MapDevices(_memoryMapper);

        // Initialize the components
        Cpu = config.ExecutionMode switch
        {
            ExecutionMode.JustInTime =>
                Cpu = config.Version.Is64Bit()
                    ? new MipsJitCpu<ulong>(config, bus)
                    : new MipsJitCpu<uint>(config, bus),

            ExecutionMode.Interpret or _ =>
                Cpu = config.Version.Is64Bit()
                    ? new MipsInterpretCpu<ulong>(config, bus)
                    : new MipsInterpretCpu<uint>(config, bus),
        };

        Cpu.ShutdownRequested += Processor_ShutdownRequested;
    }

    /// <inheritdoc/>
    public override MipsEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override IMipsCpu Cpu { get; }

    /// <inheritdoc/>
    public override MemorySystem Memory => Cpu.Memory;

    /// <inheritdoc/>
    public override IEnumerable<IDevice> Devices => _memoryMapper.Devices;

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        // System RAM
        mapper.MapDevice(0x0000_0000, new RamDevice(0x1_0000_0000)); // TODO: Config ram size

        // Graphics Buffer 
        //mapper.MapDevice(0x1300_0000, new ZaremGBU());
    }

    private void Processor_ShutdownRequested(object? sender, EventArgs e)
    {
        Cpu.ShutdownRequested -= Processor_ShutdownRequested;
        RequestShutdown();
    }
}
