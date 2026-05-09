// Avishai Dernis 2026

using Zarem.Debugger;
using Zarem.Emulator.Machine;

namespace Zarem.Components.Interfaces;

/// <summary>
/// An interface for a component of a <see cref="Project"/> that attaches debuggers.
/// </summary>
public interface IDebugComponent : IProjectComponent
{
    /// <summary>
    /// Attaches a debugger to a computer.
    /// </summary>
    Zebugger AttachDebugger(IComputer computer);
}
