// Avishai Dernis 2026

using System;
using System.Text;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.TrapHandlers.Interfaces;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A <see cref="ITrapHandler"/> that implements the SPIM syscalls.
/// </summary>
public class SpimTrapHandler : ITrapHandler
{
    /// <inheritdoc/>
    public void HandleTrap(ITrapContext context)
    {
        if (context is not MipsTrapContext mipsContext)
            throw new ArgumentException(nameof(context));

        switch ((MipsTrap)context.TrapCode)
        {
            case MipsTrap.Syscall: HandleSyscall(mipsContext.SyscallId, mipsContext); break;
            default: throw new ArgumentException(nameof(context));
        }
    }

    /// <inheritdoc/>
    private static void HandleSyscall(ulong code, MipsTrapContext context)
    {
        switch (code)
        {
            case 1: TrapHandlerCommon.PrintInteger(context); break;
            case 2: TrapHandlerCommon.PrintFloat(context); break;
            case 3: TrapHandlerCommon.PrintDouble(context); break;
            case 4: TrapHandlerCommon.PrintString(context, Encoding.ASCII); break;
            case 5: TrapHandlerCommon.ReadInteger(context); break;
            case 6: TrapHandlerCommon.ReadFloat(context); break;
            case 7: TrapHandlerCommon.ReadDouble(context); break;
            case 8: TrapHandlerCommon.ReadString(context, Encoding.ASCII); break;
            case 10: context.Cpu.RequestShutdown(); break;
            default: throw new InvalidSyscallException(context.Cpu.ProgramCounter, code);
        }
    }
}
