// Avishai Dernis 2026

using System;
using System.Text;
using Zarem.Emulator.Exceptions;
using Zarem.Emulator.TrapHandlers.Base;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// The Zarem default trap handler.
/// </summary>
public class ZaremTrapHandler : MipsTrapHandler
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
            case 80:    PrintString(context, Encoding.BigEndianUnicode); break;
            case 81:    ReadString(context, Encoding.BigEndianUnicode); break;
            default:    throw new InvalidSyscallException(context.Cpu.ProgramCounter, code);
        }
    }
}
