// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An interface for an interpreter, which handles traps as the host-layer
/// </summary>
public abstract class MipsTrapHandler : ITrapHandler
{
    /// <summary>
    /// A method to handle syscalls.
    /// </summary>
    /// <param name="code">The syscall code.</param>
    /// <param name="context">The trap context</param>
    protected abstract void HandleSyscall(ulong code, MipsTrapContext context);

    /// <summary>
    /// A method to direct trap handling.
    /// </summary>
    /// <param name="context">The context of the trap.</param>
    protected virtual void HandleTrap(MipsTrapContext context)
    {
        if ((MipsTrap)context.TrapCode is MipsTrap.Syscall)
        {
            HandleSyscall(context.V0, context);
        }
    }

    /// <inheritdoc/>
    public void HandleTrap(IComputer computer, ulong trapCode)
    {
        if (computer is not MipsComputer mipsComputer)
        {
            ThrowHelper.ThrowArgumentException(nameof(mipsComputer));
            return;
        }

        HandleTrap(new MipsTrapContext(mipsComputer, trapCode));
    }
}
