// Avishai Dernis 2026

using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An interface for a component to view computer status during debugging.
/// </summary>
public interface IDebugViewer
{
    /// <summary>
    /// Gets the registers for the debug view.
    /// </summary>
    IRegisterGroup Registers { get; }

    ///// <summary>
    ///// Gets the register groups for the debug view.
    ///// </summary>
    //IEnumerable<IRegisterGroup> RegisterGroups { get; }

    /// <summary>
    /// Creates a new debug viewer around a computer.
    /// </summary>
    /// <param name="computer">The computer to view.</param>
    /// <returns>A new <see cref="IDebugViewer"/> for the computer.</returns>
    abstract static IDebugViewer? Create(IComputer computer);
}
