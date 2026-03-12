// Avishai Dernis 2026

using Zarem.Emulator.Events;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An interface for an interpreter, which handles traps as the host-layer
/// </summary>
public abstract class MipsTrapHandler : TrapHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsTrapHandler"/> class
    /// </summary>
    public MipsTrapHandler(MipsComputer computer)
    {
        Computer = computer;

        // Register for the trap event
        Computer.Processor.TrapOccurred += OnTrap;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MipsTrapHandler"/> class.
    /// </summary>
    ~MipsTrapHandler()
    {
        // Unregister the trap event
        Computer.Processor.TrapOccurred -= OnTrap;
    }

    /// <summary>
    /// Gets the computer the traps occur on.
    /// </summary>
    protected MipsComputer Computer { get; }

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    protected uint A0 => Computer.Processor[GPRegister.Argument0];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    protected uint A1 => Computer.Processor[GPRegister.Argument1];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    protected uint A2 => Computer.Processor[GPRegister.Argument2];

    /// <summary>
    /// Gets the value of first argument register.
    /// </summary>
    protected uint A3 => Computer.Processor[GPRegister.Argument3];

    /// <summary>
    /// Gets or sets the value of first return value register.
    /// </summary>
    protected uint V0
    {
        get => Computer.Processor[GPRegister.ReturnValue0];
        set => Computer.Processor[GPRegister.ReturnValue0] = value;
    }

    /// <summary>
    /// A method to direct trap handling.
    /// </summary>
    /// <param name="trap">The type of trap that occurred.</param>
    protected abstract void HandleTrap(MipsTrap trap);

    private void OnTrap(ICpu sender, TrapEventArgs e)
    {
        if (sender is not MipsCpu cpu)
            return;

        // The emulator is handling the trap
        // No need to interpret
        if (!e.Unhandled)
            return;

        if ((MipsTrap)e.Trap is MipsTrap.Syscall)
        {
            HandleSyscall(cpu.RegisterFile[GPRegister.ReturnValue0]);
        }

        // Resume the emulation
        e.Resume();
    }
}
