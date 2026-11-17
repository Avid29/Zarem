// Avishai Dernis 2026

using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;

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
                HandleModeWrite(value);
                break;

            case MiRegister.InterruptMask:
                HandleInterruptMaskWrite(value);
                break;
        }
    }

    private void HandleModeWrite(uint value)
    {
        // MI_MODE_REG bit operations (e.g., setting/clearing init mode, EBRAM size, etc.)
        // Bit 7: clear init mode, Bit 8: set init mode, etc.
        uint currentMode = Registers.Read<uint>((ulong)MiRegister.Mode);

        // Apply bit modifications per N64 hardware specs if needed
        Registers.Write((ulong)MiRegister.Mode, value);
    }

    private void HandleInterruptMaskWrite(uint value)
    {
        // MI_INTR_MASK_REG handles setting and clearing mask bits via individual bits
        // Bits 0-5 set/clear masks for SP, SI, AI, VI, PI, DP interrupts
        Registers.Write((ulong)MiRegister.InterruptMask, value);
    }
}
