// Avishai Dernis 2026

using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;
using Zarem.N64.Models.MipsInterface;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-component of the <see cref="RealityCoProcessor"/> responsible for handling MIPS Interface (MI) control, version, and interrupt routing.
/// </summary>
public class N64MipsInterface : N64RegisterDeviceBase<MiRegister>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="N64MipsInterface"/> class.
    /// </summary>
    public N64MipsInterface() : base(new FlatMemoryStore(0x10))
    {
        // Initialize standard default version (e.g., standard N64 MI version ID)
        Registers.Write((ulong)MiRegister.Version, 0x0202_0102);
    }

    /// <inheritdoc/>
    public override string Name => "N64 MIPS Interface (MI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0430_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x10;

    /// <inheritdoc/>
    protected override bool HasWriteSideEffects(MiRegister register, out bool directWrite)
    {
        bool special = register is MiRegister.Mode or MiRegister.InterruptMask;
        directWrite = !special;
        return special;
    }

    /// <inheritdoc/>
    protected override void HandleWriteSideEffects(MiRegister register, uint value)
    {
        switch (register)
        {
            case MiRegister.Mode:
                HandleModeWrite((ModeCommand)value);
                break;

            case MiRegister.InterruptMask:
                HandleInterruptMaskWrite((N64InterruptCommand)value);
                break;
        }
    }

    private void HandleModeWrite(ModeCommand command)
    {
        var mode = (Mode)Registers.Read<uint>((ulong)MiRegister.Mode);

        // Update repeat
        if (command.ClearRepeat) mode.Repeat = false;
        if (command.SetRepeat)
        {
            mode.Repeat = true;
            mode.RepeatCount = command.RepeatCount;
        }

        // Update EBus
        if (command.ClearEBus) mode.EBus = false;
        if (command.SetEBus) mode.EBus = true;

        //if (command.ClearDP) TODO: What?

        // Update Upper
        if (command.ClearUpper) mode.Upper = false;
        if (command.SetUpper) mode.Upper = true;

        // Writeback
        Registers.Write((ulong)MiRegister.Mode, (uint)mode);
    }

    private void HandleInterruptMaskWrite(N64InterruptCommand command)
    {
        var flags = (N64InterruptFlags)Registers.Read<uint>((ulong)MiRegister.Mode);

        // Update RSP
        if (command.HasFlag(N64InterruptCommand.ClearRealitySignalProcessor)) flags &= ~N64InterruptFlags.RealitySignalProcessor;
        if (command.HasFlag(N64InterruptCommand.SetRealitySignalProcessor)) flags |= N64InterruptFlags.RealitySignalProcessor;

        // Update SI
        if (command.HasFlag(N64InterruptCommand.ClearSerialInterface)) flags &= ~N64InterruptFlags.SerialInterface;
        if (command.HasFlag(N64InterruptCommand.SetSerialInterface)) flags |= N64InterruptFlags.SerialInterface;

        // Update AI
        if (command.HasFlag(N64InterruptCommand.ClearAudioInterace)) flags &= ~N64InterruptFlags.AudioInterace;
        if (command.HasFlag(N64InterruptCommand.SetAudioInterace)) flags |= N64InterruptFlags.AudioInterace;

        // Update VI
        if (command.HasFlag(N64InterruptCommand.ClearVideoInterface)) flags &= ~N64InterruptFlags.VideoInterface;
        if (command.HasFlag(N64InterruptCommand.SetVideoInterface)) flags |= N64InterruptFlags.VideoInterface;

        // Update PI
        if (command.HasFlag(N64InterruptCommand.ClearParallelInterface)) flags &= ~N64InterruptFlags.ParallelInterface;
        if (command.HasFlag(N64InterruptCommand.SetParallelInterface)) flags |= N64InterruptFlags.ParallelInterface;

        // Update RDP
        if (command.HasFlag(N64InterruptCommand.ClearRealityDisplayProcessor)) flags &= ~N64InterruptFlags.RealityDisplayProcessor;
        if (command.HasFlag(N64InterruptCommand.SetRealityDisplayProcessor)) flags |= N64InterruptFlags.RealityDisplayProcessor;

        // Writeback
        Registers.Write((ulong)MiRegister.InterruptMask, (uint)flags);
    }
}
