// Avishai Dernis 2026

using Zarem.Components.Interfaces;
using Zarem.Debugger;
using Zarem.Debugger.Handlers;
using Zarem.Descriptors;
using Zarem.Emulator.Machine;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> that attaches debuggers.
/// </summary>
internal class DebugComponent<THandler> : IDebugComponent
    where THandler : IDebugHandler
{
    private readonly THandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugComponent{THandler}"/> class.
    /// </summary>
    public DebugComponent(THandler handler, IDebuggerDescriptor descriptor)
    {
        _handler = handler;
    }

    /// <inheritdoc/>
    public Zebugger AttachDebugger(IComputer computer) => new Zebugger(_handler, computer);
}
