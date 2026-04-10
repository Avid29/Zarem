// Avishai Dernis 2026

using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An <see cref="IDebugViewer"/> for the <see cref="RiscVComputer"/>
/// </summary>
public class RiscVDebugViewer : IDebugViewer
{
    private readonly RiscVComputer _riscVComputer;

    private RiscVDebugViewer(RiscVComputer riscVComputer)
    {
        _riscVComputer = riscVComputer;

        Registers = new RiscVRegisterViewer(_riscVComputer.Cpu.RegisterFile, RiscVRegisterSet.GeneralPurpose);
    }

    /// <inheritdoc/>
    public IRegisterGroup Registers { get; }

    /// <inheritdoc/>
    public static IDebugViewer? Create(IComputer computer)
    {
        if (computer is not RiscVComputer mips)
            return null;

        return new RiscVDebugViewer(mips);
    }
}
