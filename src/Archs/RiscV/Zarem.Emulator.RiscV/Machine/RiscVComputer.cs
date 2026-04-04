// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Devices;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the RISC-V interpreter.
/// </summary>
public class RiscVComputer : ComputerBase
{
    private readonly MemoryMapper _memoryMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVComputer"/> class.
    /// </summary>
    public RiscVComputer(RiscVEmulatorConfig config)
    {
        Config = config;

        // Create the physical memory bus
        _memoryMapper = new MemoryMapper();
        var bus = new PhysicalBus(_memoryMapper);
        MapDevices(_memoryMapper);

        Cpu = config.VersionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => new RiscVCpu<uint>(config, bus),
            RiscVBaseVersion.RV64 => new RiscVCpu<ulong>(config, bus),
            RiscVBaseVersion.RV128 => new RiscVCpu<UInt128>(config, bus),
            _ => throw new NotImplementedException()
        };
    }

    /// <inheritdoc/>
    public override RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override IRiscVCpu Cpu { get; }

    /// <inheritdoc/>
    public override IMemorySystem Memory => Cpu.Memory;

    /// <inheritdoc/>
    public override IEnumerable<IDevice> Devices => _memoryMapper.Devices;

    /// <inheritdoc/>
    public override void Tick() => Cpu.Step();

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        // System RAM
        mapper.MapDevice(0x0000_0000, new RamDevice(1024 * 1024 * 1024)); // TODO: Config ram size
    }
}
