// Avishai Dernis 2026

using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Extensions.System.IO;
using Zarem.Linker.Extensions;
using Zarem.Linker.Handlers;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Linker.Handler;

/// <summary>
/// A linker handler for the MIPS architecture.
/// </summary>
public class MIPSLinkerHandler : ILinkerHandler
{
    /// <inheritdoc/>
    public bool PatchRelocation(Section section, RelocationEntry relocation, ulong symbolAddress, ulong place, ILogger? logger = null)
    {
        section.Position = (long)(place - section.VirtualAddress);

        section.Stream.TryRead<uint>(out var value);
        var instruction = (MIPSInstruction)value;

        long target = (long)symbolAddress + relocation.Addend;
        long pcTarget = target - ((long)place + 4);

        switch ((MipsReferenceType)relocation.Type)
        {
            // R_MIPS_LO16
            case MipsReferenceType.Low16:
                instruction.ImmediateValue = (short)(target & 0xFFFF);
                value = (uint)instruction;
                break;

            // R_MIPS_HI16
            case MipsReferenceType.High16:
                instruction.ImmediateValue = (short)((target + 0x8000) >> 16);
                value = (uint)instruction;
                break;

            // R_MIPS_PC16
            case MipsReferenceType.PCRelative16:
                instruction.Offset = (int)pcTarget;
                value = (uint)instruction;
                break;

            // R_MIPS_26
            case MipsReferenceType.JumpTarget26:
                if (((ulong)target & 0xF0000000UL) != (place & 0xF0000000UL))
                {
                    // TODO: Log error, jump out of range. 
                    logger?.Log(Severity.Error, LogId.JumpOutOfRange, "TODO: Get file name", "JumpOutOfRange", relocation.SymbolName);
                }

                instruction.Address = (uint)target;
                value = (uint)instruction;
                break;

            // R_MIPS_32
            case MipsReferenceType.Absolute32:
                value = (uint)target;
                break;

            default:
                return false;
        }

        section.Position -= sizeof(uint);
        section.Stream.TryWrite(value);
        return true;
    }
}
