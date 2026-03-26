// Avishai Dernis 2025

using Zarem.Assembler.Config;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Meta;
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
    protected override void LoadInstruction(MipsInstructionMetaBase metadata)
    {
        if (!metadata.IsValidFor(Config.MipsVersion))
            return;

        // We can simplify the logic by centralizing the key generation
        // using a more descriptive set of fields in DisassemblerLookup.
        DisassemblerLookup? key = metadata switch
        {
            RTypeInstructionMeta r => new DisassemblerLookup(
                (byte)r.OperationCode,
                (byte)r.FuncCode),

            RegImmInstructionMeta ri => new DisassemblerLookup(
                (byte)OperationCode.RegisterImmediate,
                (byte)ri.RtCode),

            CoProc0InstructionsMeta c0 => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor0,
                (byte)c0.RSCode, // sub-opcode
                (byte?)c0.FuncCode ?? 255,
                c0.FixedRD), // Match specific RD (like 1 for eretnc)

            CoProc1InstructionsMeta c1 => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor1,
                (byte)c1.RSCode),

            FloatInstructionMeta f => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor1,
                (byte)f.Function,
                isFloatFunc: true), // New flag to check bits [5:0] instead of [25:21]

            ITypeInstructionMeta std => new DisassemblerLookup(
                (byte)std.OperationCode),

            _ => null
        };

        if (key is not null)
        {
            LoadInstruction(key.Value, metadata);
        }
    }
}
