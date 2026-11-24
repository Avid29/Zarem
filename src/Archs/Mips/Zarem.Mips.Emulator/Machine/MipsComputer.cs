// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Zarem.Emulator.Config.Enums;
using Zarem.Emulator.Devices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Memory;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Versioning.Enums;
using Zarem.Models;
using Zarem.Models.Enums;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A class representing a computer system in the MIPS interpreter.
/// </summary>
public class MipsComputer : ComputerBase
{
    private readonly MemoryMapper _memoryMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsComputer"/> class.
    /// </summary>
    public MipsComputer(MipsEmulatorConfig config, bool mapDevices = true)
    {
        Config = config;

        // Create the physical memory bus
        _memoryMapper = new MemoryMapper();
        var bus = new PhysicalBus(_memoryMapper, Endianness.Big);

        // Construct the CPU
        Cpu = config.VersionInfo.Is64Bit
            ? new MipsCpu<ulong>(config, bus)
            : new MipsCpu<uint>(config, bus);

        Cpu.ShutdownRequested += Processor_ShutdownRequested;

        if (mapDevices)
        {
            MapDevices(_memoryMapper);
        }
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
    public override void Reset()
    {
        Cpu.ProgramCounter = MipsCpu<uint>.BOOT_ADDRESS;
    }

    /// <inheritdoc/>
    protected override void SetupUserSpaceMapping(Module module)
    {
        var c0 = Cpu.ControlCoProcessor;
        Guard.IsNotNull(c0);

        // Force the CPU status register into User Mode privileges instantly
        c0.PrivilegeMode = PrivilegeMode.User;

        // Iterate through the module's requirements or iterate a block allocations 
        // pattern matching the exact sizes of text/data/stack sections into the CPU's TLB.
        int i = 0;
        foreach (var section in module.Sections.Values)
        {
            ulong startVAddr = section.VirtualAddress;
            ulong size = (ulong)section.Stream.Length;

            // Write the required Tlb entries matching 'startVAddr' to back 
            // this specific segment with physical memory frames...
            i += c0.Tlb.InitilizeSegment(i, startVAddr, size);
        }

        // Also inject mappings dedicated to the Stack segment framework (near 0x7FFF_8000)
        c0.Tlb.InitilizeSegment(i, 0x7FFF_0000, 0x8000);
    }

    /// <summary>
    /// Maps the devices to the memory mapper.
    /// </summary>
    protected void RemapDevices()
    {
        _memoryMapper.Clear();
        MapDevices(_memoryMapper);
    }

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
