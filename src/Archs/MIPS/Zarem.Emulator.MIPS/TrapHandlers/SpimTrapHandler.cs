// Avishai Dernis 2026

using System.Text;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.TrapHandlers.Base;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A <see cref="MipsTrapHandler"/> that implements the SPIM syscalls.
/// </summary>
public class SpimTrapHandler : MipsTrapHandler
{
    /// <inheritdoc/>
    protected override void HandleSyscall(ulong code, MipsTrapContext context)
    {
        switch (code)
        {
            case 1:     PrintInteger(context); break;
            case 2:     PrintFloat(context); break;
            case 3:     PrintDouble(context); break;
            case 4:     PrintString(context, Encoding.ASCII); break;
            case 5:     ReadInteger(context); break;
            case 6:     ReadFloat(context); break;
            case 7:     ReadDouble(context); break;
            case 8:     ReadString(context, Encoding.ASCII); break;
            case 10:    Shutdown(context); break;
            default: throw new InvalidSyscallException(context.Cpu.ProgramCounter, code);
        }
    }
}
