// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Devices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Memory;
using Zarem.Models.Enums;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.JIT;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Machine;

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

        // Determine gpr size
        var gprType = config.VersionInfo.Base switch
        {
            RiscVBaseVersion.RV32 => typeof(uint),
            RiscVBaseVersion.RV64 => typeof(ulong),
            RiscVBaseVersion.RV128 => typeof(UInt128),
            _ => throw new NotSupportedException()
        };

        // Determine float reg size
        var extensions = Config.VersionInfo.Extensions;
        var floatType = typeof(byte); // Sentinel type meaning no float extension

        if (extensions.HasFlag(RiscVExtensions.QuadrupleFloatingPoint)) floatType = typeof(UInt128);
        else if (extensions.HasFlag(RiscVExtensions.DoubleFloatingPoint)) floatType = typeof(ulong);
        else if (extensions.HasFlag(RiscVExtensions.SingleFloatingPoint)) floatType = typeof(uint);
        // Half should not be possible, since it should always require single. Best to be careful though
        else if (extensions.HasFlag(RiscVExtensions.HalfPrecisionFloatingPoint)) floatType = typeof(ushort);

        // Determine the CPU implementation type
        var cpuType = config.ExecutionMode switch
        {
            ExecutionMode.Interpret => typeof(RiscVInterpretCpu<,>),
            ExecutionMode.JustInTime => typeof(RiscVJitCpu<,>),
            _ => throw new NotImplementedException(),
        };

        // Construct the CPU
        var closedCpuType = cpuType.MakeGenericType(gprType, floatType);
        var cpu = (IRiscVCpu?)Activator.CreateInstance(closedCpuType, config, bus);
        Guard.IsNotNull(cpu);
        Cpu = cpu;

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
