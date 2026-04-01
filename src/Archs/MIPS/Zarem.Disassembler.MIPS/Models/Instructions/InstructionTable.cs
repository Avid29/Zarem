// Avishai Dernis 2025

using Zarem.Assembler;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Meta;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Disassembler.Models.Instructions;

/// <summary>
/// A class for managing instruction lookup by opcode and function code.
/// </summary>
public class InstructionTable : MipsInstructionTableBase<DisassemblerLookup>
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
        if (!metadata.IsValidFor(Config.Version))
            return;

        // We can simplify the logic by centralizing the key generation
        // using a more descriptive set of fields in DisassemblerLookup.
        DisassemblerLookup? key = metadata switch
        {
            RTypeInstructionMeta r => new DisassemblerLookup(
                (byte)r.OperationCode,
                (byte)r.FuncCode),

            JTypeInstructionMeta j => new DisassemblerLookup(
                (byte)j.OperationCode),

            RegImmInstructionMeta ri => new DisassemblerLookup(
                (byte)OperationCode.RegisterImmediate,
                (byte)ri.RtCode),

            CoProc0InstructionsMeta c0 => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor0,
                (byte)c0.RSCode, // sub-opcode
                c0.RSCode switch
                {
                    CoProc0RSCode.C0 => (byte?)c0.FuncCode,
                    CoProc0RSCode.MFMC0 => (byte?)c0.Mfmc0FuncCode,
                    _ => null,
                } ?? 255,
                IsFloat: c0.FixedRD is 1), // Match specific RD (like 1 for eretnc)

            CoProc1InstructionsMeta c1 => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor1,
                (byte)c1.RSCode),

            FloatInstructionMeta f => new DisassemblerLookup(
                (byte)OperationCode.Coprocessor1,
                (byte)f.Function,
                IsFloat: true), // New flag to check bits [5:0] instead of [25:21]

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
