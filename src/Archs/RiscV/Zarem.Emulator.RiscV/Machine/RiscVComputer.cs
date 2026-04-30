// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Emulator.Config;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Devices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.JIT;
using Zarem.Models.Enums;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the RISC-V interpreter.
/// </summary>
public sealed class RiscVComputer : ComputerBase
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
        var bus = new PhysicalBus(_memoryMapper, Endianness.Little);
        MapDevices(_memoryMapper);

        Cpu = config.ExecutionMode switch
        {
            ExecutionMode.Interpret => config.VersionInfo.Base switch
            {
                RiscVBaseVersion.RV32 => new RiscVInterpretCpu<uint>(config, bus),
                RiscVBaseVersion.RV64 => new RiscVInterpretCpu<ulong>(config, bus),
                RiscVBaseVersion.RV128 => new RiscVInterpretCpu<UInt128>(config, bus),
                _ => throw new NotImplementedException(),
            },
            ExecutionMode.JustInTime => config.VersionInfo.Base switch
            {
                RiscVBaseVersion.RV32 => new RiscVJitCpu<uint>(config, bus),
                RiscVBaseVersion.RV64 => new RiscVJitCpu<ulong>(config, bus),
                RiscVBaseVersion.RV128 => new RiscVJitCpu<UInt128>(config, bus),
                _ => throw new NotImplementedException(),
            },
            _ => throw new NotImplementedException(),
        };

        Cpu.ShutdownRequested += Processor_ShutdownRequested;
    }

    /// <inheritdoc/>
    public override RiscVEmulatorConfig Config { get; }

    /// <inheritdoc/>
    public override IRiscVCpu Cpu { get; }

    /// <inheritdoc/>
    public override MemorySystem Memory => Cpu.Memory;

    /// <inheritdoc/>
    public override IEnumerable<IDevice> Devices => _memoryMapper.Devices;

    /// <inheritdoc/>
    protected override void MapDevices(MemoryMapper mapper)
    {
        // System RAM
        mapper.MapDevice(0x0000_0000, new RamDevice(0x1_0000_0000)); // TODO: Config ram size
    }

    private void Processor_ShutdownRequested(object? sender, EventArgs e)
    {
        Cpu.ShutdownRequested -= Processor_ShutdownRequested;
        RequestShutdown();
    }
}
