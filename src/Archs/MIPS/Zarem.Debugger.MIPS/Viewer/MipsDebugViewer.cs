// Avishai Dernis 2026

using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An <see cref="IDebugViewer"/> for the <see cref="MipsComputer"/>
/// </summary>
public class MipsDebugViewer : IDebugViewer
{
    private readonly MipsComputer _mipsComputer;

    private MipsDebugViewer(MipsComputer mipsComputer)
    {
        _mipsComputer = mipsComputer;

        Registers = new MipsRegisterViewer(_mipsComputer.Processor.RegisterFile, RegisterSet.GeneralPurpose);
    }

    /// <inheritdoc/>
    public IRegisterGroup Registers { get; }

    /// <inheritdoc/>
    public static IDebugViewer? Create(IComputer computer)
    {
        if (computer is not MipsComputer mips)
            return null;

        return new MipsDebugViewer(mips);
    }
}
