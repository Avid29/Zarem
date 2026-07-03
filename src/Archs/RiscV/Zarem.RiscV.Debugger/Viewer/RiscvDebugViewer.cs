// Avishai Dernis 2026

using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Zarem.RiscV.Debugger.Viewer;

/// <summary>
/// An <see cref="IDebugViewer"/> for the <see cref="RiscVComputer"/>
/// </summary>
public class RiscVDebugViewer : IDebugViewer
{
    private readonly RiscVComputer _riscVComputer;

    private RiscVDebugViewer(RiscVComputer riscVComputer)
    {
        _riscVComputer = riscVComputer;

        RegisterViewer = new RiscVRegisterViewer(_riscVComputer.Cpu.RegisterFile, RiscVRegisterSet.GeneralPurpose);
    }

    /// <inheritdoc/>
    public IRegisterViewer RegisterViewer { get; }

    /// <inheritdoc/>
    public static IDebugViewer? Create(IComputer computer)
    {
        if (computer is not RiscVComputer mips)
            return null;

        return new RiscVDebugViewer(mips);
    }
}
