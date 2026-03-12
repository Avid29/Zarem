// Avishai Dernis 2025

using Zarem.Assembler.Config;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Disassembler.Models.Instructions;

/// <summary>
/// A class for managing instruction lookup by opcode and function code.
/// </summary>
public class InstructionTable : InstructionTableBase<DisassemblerLookup>
{   
    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionTable"/> class.
    /// </summary>
    public InstructionTable(MipsAssemblerConfig config) : base(config)
    {
    }

    /// <inheritdoc/>
    protected override void LoadInstruction(MipsInstructionMetadata metadata)
    {
        if (!metadata.MipsVersions.Contains(Config.MipsVersion))
            return;

        byte? funcCode = metadata.Type switch
        {
            InstructionType.BasicR => (byte?)metadata.FuncCode,

            InstructionType.BasicI or
            InstructionType.BasicJ => 0,

            InstructionType.Special2R => (byte?)metadata.Function2Code,
            InstructionType.Special3R => (byte?)metadata.Function3Code,

            InstructionType.RegisterImmediate or
            InstructionType.RegisterImmediateBranch => (byte?)metadata.RegisterImmediateFuncCode,

            InstructionType.Coproc0 => GetRSCode(metadata),

            InstructionType.Coproc1 => (byte?)metadata.CoProc1RS,
            InstructionType.Float => (byte?)metadata.FloatFuncCode,
            _ => 0,
        };

        byte? funcCode2 = metadata.Type switch
        {
            InstructionType.Coproc0 => funcCode switch
            {
                (byte)CoProc0RSCode.C0 => (byte?)metadata.Co0FuncCode,
                (byte)CoProc0RSCode.MFMC0 => (byte?)metadata.Mfmc0FuncCode,
                _ => 255,
            },
            _ => 255,
        };

        if (metadata.OpCode is null || funcCode is null || funcCode2 is null)
            return;

        bool hasFormat = metadata.FloatFormats is not null;
        bool eretnc = metadata.Co0FuncCode is Co0FuncCode.ExceptionReturn && metadata.RD is 1;

        var key = new DisassemblerLookup((byte)metadata.OpCode, (byte)funcCode, (byte)funcCode2, hasFormat || eretnc);
        LoadInstruction(key, metadata);
    }

    private static byte GetRSCode(MipsInstructionMetadata metadata)
    {
        if (metadata.OpCode is not OperationCode.Coprocessor0)
            return 0;

        if (metadata.CoProc0RS is not null)
            return (byte)metadata.CoProc0RS;

        if (metadata.Co0FuncCode is not null)
            return (byte)CoProc0RSCode.C0;

        if (metadata.Mfmc0FuncCode is not null)
            return (byte)CoProc0RSCode.MFMC0;

        return 0;
    }
}
