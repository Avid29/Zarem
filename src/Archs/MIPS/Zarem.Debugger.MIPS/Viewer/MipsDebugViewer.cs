// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An <see cref="IDebugViewer"/> for the <see cref="MipsComputer"/>
/// </summary>
public class MipsDebugViewer : IDebugViewer
{
    private MipsComputer _mipsComputer;

    private MipsDebugViewer(MipsComputer mipsComputer)
    {
        _mipsComputer = mipsComputer;

        RegisterGroups =
            [
                new MipsRegisterViewer(_mipsComputer.Processor.RegisterFile),
                new MipsRegisterViewer(_mipsComputer.Processor.CoProcessor0.RegisterFile),
            ];
    }

    /// <inheritdoc/>
    public IEnumerable<IRegisterGroup> RegisterGroups { get; }

    /// <inheritdoc/>
    public static IDebugViewer? Create(IComputer computer)
    {
        if (computer is not MipsComputer mips)
            return null;

        return new MipsDebugViewer(mips);
    }
}
