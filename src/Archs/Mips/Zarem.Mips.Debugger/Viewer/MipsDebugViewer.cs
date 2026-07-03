// Avishai Dernis 2026

using Zarem.Debugger.Viewer;
using Zarem.Emulator.Machine;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Debugger.Viewer;

/// <summary>
/// An <see cref="IDebugViewer"/> for the <see cref="MipsComputer"/>
/// </summary>
public class MipsDebugViewer : IDebugViewer
{
    private readonly MipsComputer _mipsComputer;

    private MipsDebugViewer(MipsComputer mipsComputer)
    {
        _mipsComputer = mipsComputer;

        RegisterViewer = new MipsRegisterViewer(_mipsComputer.Cpu.RegisterFile, MipsRegisterSet.GeneralPurpose);
    }

    /// <inheritdoc/>
    public IRegisterViewer RegisterViewer { get; }

    /// <inheritdoc/>
    public static IDebugViewer? Create(IComputer computer)
    {
        if (computer is not MipsComputer mips)
            return null;

        return new MipsDebugViewer(mips);
    }
}
