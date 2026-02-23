// Avishai Dernis 2026

using Zarem.Emulator.Events;
using Zarem.Emulator.Executor.Enum;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.CPU;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// An interface for an interpreter, which handles traps as the host-layer
/// </summary>
public abstract class MIPSTrapHandler : TrapHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSTrapHandler"/> class
    /// </summary>
    public MIPSTrapHandler(MIPSComputer computer)
    {
        Computer = computer;

        // Register for the trap event
        Computer.Processor.TrapOccurring += Processor_TrapOccurring;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="MIPSTrapHandler"/> class.
    /// </summary>
    ~MIPSTrapHandler()
    {
        // Unregister the trap event
        Computer.Processor.TrapOccurring -= Processor_TrapOccurring;
    }

    /// <summary>
    /// Gets the computer the traps occur on.
    /// </summary>
    protected MIPSComputer Computer { get; }

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
    protected abstract void HandleTrap(MIPSTrap trap);

    private void Processor_TrapOccurring(MIPSCpu sender, TrapOccurringEventArgs<MIPSTrap> e)
    {
        // The emulator is handling the trap
        // No need to interpret
        if (!e.Unhandled)
            return;

        if (e.Trap is MIPSTrap.Syscall)
        {
            HandleSyscall(sender.RegisterFile[GPRegister.ReturnValue0]);
        }

        // Resume the emulation
        e.Resume();
    }
}
