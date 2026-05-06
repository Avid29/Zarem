// Avishai Dernis 2025

using System.Text;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Models.Tables;
using Zarem.Mips.Assembler;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Disassembler;

/// <summary>
/// A MIPS disassembler.
/// </summary>
public partial class MipsDisassembler
{
    private readonly MipsInstructionDecodeTable<MipsInstructionMetaBase?> _instructionTable;
    private readonly FormatTable<MipsFloatFormat> _formatTable = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsDisassembler"/> class.
    /// </summary>
    public MipsDisassembler(MipsAssemblerConfig config)
    {
        _instructionTable = new MipsInstructionDecodeTable<MipsInstructionMetaBase?>(null);
        Initialize(config);
    }

    /// <summary>
    /// Disassembles the <paramref name="instruction"/> into a string.
    /// </summary>
    /// <param name="instruction">The instruction to disassemble.</param>
    /// <returns>The instruction as a string.</returns>
    public string Disassemble(MipsInstruction instruction)
    {
        var meta = _instructionTable.Lookup(instruction);
        if (meta is null)
        {
            return "Unknown instruction";
        }

        // Apply the format to the name if it exists
        var name = meta.Name;
        if (meta is MipsFloatInstructionMeta)
        {
            var format = ((MipsFloatInstruction)instruction).Format;
            name = _formatTable.ApplyFormat(name, format);
        }

        StringBuilder pattern = new($"{name}");

        if (meta.ArgumentCount is not 0)
            pattern.Append(' ');

        for (int i = 0; i < meta.ArgumentPattern.Length; i++)
        {
            pattern.Append(meta.ArgumentPattern[i] switch
            {
                MipsArgument.RS => GetRegisterString(instruction.RS, MipsRegisterSet.GeneralPurpose),
                MipsArgument.RT => GetRegisterString(instruction.RT, MipsRegisterSet.GeneralPurpose),
                MipsArgument.RD => GetRegisterString(instruction.RD, MipsRegisterSet.GeneralPurpose),
                MipsArgument.ShiftAmount => instruction.ShiftAmount,
                MipsArgument.Immediate => instruction.Immediate,
                MipsArgument.Offset => instruction.Offset,
                MipsArgument.LargeOffset => instruction.Address,
                MipsArgument.Address => instruction.Address,
                MipsArgument.AddressBase => $"{instruction.Immediate}({GetRegisterString(instruction.RS, MipsRegisterSet.GeneralPurpose)})",
                MipsArgument.FullImmediate => 0, // Won't happen until pseudo-instruction disassembly
                MipsArgument.FS => GetRegisterString((MipsGpRegister)((MipsFloatInstruction)instruction).FS, MipsRegisterSet.FloatingPoints),
                MipsArgument.FT => GetRegisterString((MipsGpRegister)((MipsFloatInstruction)instruction).FT, MipsRegisterSet.FloatingPoints),
                MipsArgument.FD => GetRegisterString((MipsGpRegister)((MipsFloatInstruction)instruction).FD, MipsRegisterSet.FloatingPoints),
                MipsArgument.RS_Numbered => GetRegisterString(instruction.RS, MipsRegisterSet.Numbered),
                MipsArgument.RT_Numbered => GetRegisterString(instruction.RT, MipsRegisterSet.Numbered),
                _ => "unknown",
            });

            if (i < meta.ArgumentPattern.Length - 1)
            {
                pattern.Append(", ");
            }
        }

        return $"{pattern}";
    }

    private void Initialize(MipsAssemblerConfig config)
    {
        var instructions = new MipsInstructionTable(config).GetInstructions();
        foreach (var instruction in instructions)
        {
            switch (instruction)
            {
                case ITypeInstructionMeta i:
                    _instructionTable.Register(i.OperationCode, i);
                    break;
                case JTypeInstructionMeta j:
                    _instructionTable.Register(j.OperationCode, j);
                    break;
                case RTypeInstructionMeta r:
                    switch (r.OperationCode)
                    {
                        case MipsOpCode.Special:
                            _instructionTable.Register(r.FuncCode, r);
                            break;
                        case MipsOpCode.Special2:
                            _instructionTable.Register((Func2Code)r.FuncCode, r);
                            break;
                        case MipsOpCode.Special3:
                            _instructionTable.Register((Func3Code)r.FuncCode, r);
                            break;
                    }
                    break;
                case RegImmInstructionMeta r:
                    _instructionTable.Register(r.RtCode, r);
                    break;
                case CoProc0InstructionsMeta c0:
                    if (c0.FuncCode.HasValue)
                    {
                        _instructionTable.Register(c0.FuncCode.Value, c0);
                    }
                    else if (c0.Mfmc0FuncCode.HasValue)
                    {
                        _instructionTable.Register(c0.Mfmc0FuncCode.Value, c0);
                    }
                    else
                    {
                        _instructionTable.Register(c0.RSCode, c0);
                    }
                    break;
                case CoProc1InstructionsMeta c1:
                    _instructionTable.Register(c1.RSCode, c1);
                    break;
                case MipsFloatInstructionMeta f:
                    _instructionTable.Register(f.Function, f);
                    break;
            }
        }
    }

    private static string GetRegisterString(MipsGpRegister register, MipsRegisterSet set) => $"${MipsRegisterTable.Instance.GetRegisterString(register, set)}";
}
