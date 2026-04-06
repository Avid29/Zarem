// Avishai Dernis 2026

using System;
using Zarem.Emulator.TrapHandlers.Interfaces;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// The Zarem default trap handler.
/// </summary>
public class ZaremTrapHandler : ITrapHandler
{
    /// <inheritdoc/>
    public void HandleTrap(ITrapContext context)
    {
        if (!context.IsSyscall)
        {
            throw new NotImplementedException();
        }

        switch(context.SyscallId)
        {
            case 1: TrapHandlerCommon.PrintInteger(context); break;
            case 2: TrapHandlerCommon.ReadInteger(context); break;
            case 3: TrapHandlerCommon.PrintString(context); break;
            case 4: TrapHandlerCommon.ReadString(context); break;
            case 5: TrapHandlerCommon.PrintFloat(context); break;
            case 6: TrapHandlerCommon.ReadFloat(context); break;
            case 7: TrapHandlerCommon.PrintDouble(context); break;
            case 8: TrapHandlerCommon.ReadDouble(context); break;
            case 9: context.Cpu.RequestShutdown(); break;
            default: throw new InvalidOperationException($"Invalid syscall ID: {context.SyscallId}");
        }

        // Increment program counter (TODO: dynamically sized instructions)
        context.Cpu.ProgramCounter += 4;
    }
}
