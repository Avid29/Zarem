// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.Linq;
using System.Text;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Models.Tables;
using Zarem.Mips.Assembler;
using Zarem.Mips.Disassembler.Models;
using Zarem.Mips.Disassembler.Models.Instructions;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Disassembler;

/// <summary>
/// A MIPS disassembler.
/// </summary>
public class MipsDisassembler
{
    private FormatTable<MipsFloatFormat> _formatTable = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsDisassembler"/> class.
    /// </summary>
    public MipsDisassembler(MipsAssemblerConfig config)
    {
        Config = config;
        InstructionTable = new InstructionTable(config);
    }

    /// <summary>
    /// Gets the assembler configuration to use for disassembly.
    /// </summary>
    public MipsAssemblerConfig Config { get; }

    /// <summary>
    /// Gets the instruction table for this disassembler instance.
    /// </summary>
    public InstructionTable InstructionTable { get; }

    /// <summary>
    /// Disassembles the <paramref name="instruction"/> into a string.
    /// </summary>
    /// <param name="instruction">The instruction to disassemble.</param>
    /// <returns>The instruction as a string.</returns>
    public string Disassemble(MipsInstruction instruction)
    {
        byte funcCode = instruction.Type switch
        {
            // Technically could be done with 'or', but clarity is nice.
            MipsInstructionType.BasicR => (byte)instruction.FuncCode,
            MipsInstructionType.Special2R => (byte)instruction.Func2Code,
            MipsInstructionType.Special3R => (byte)instruction.Func3Code,

            MipsInstructionType.BasicI or
            MipsInstructionType.BasicJ => 0,

            MipsInstructionType.RegisterImmediateTrap or
            MipsInstructionType.RegisterImmediateBranch => (byte)instruction.RTFuncCode,

            MipsInstructionType.Coproc0 => (byte)((CoProc0Instruction)instruction).CoProc0RSCode,

            MipsInstructionType.Coproc1 => (byte)((MipsFloatInstruction)instruction).RSCode,
            MipsInstructionType.Float => (byte)((MipsFloatInstruction)instruction).Function,

            _ => 255,
        };

        byte funcCode2 = instruction.Type switch
        {
            MipsInstructionType.Coproc0 => funcCode switch
            {
                (byte)CoProc0RSCode.C0 => (byte)((CoProc0Instruction)instruction).Co0FuncCode,
                (byte)CoProc0RSCode.MFMC0 => (byte)((CoProc0Instruction)instruction).MFMC0FuncCode,
                _ => 255,
            },
            _ => 255,
        };

        // If the instruction is a float instruction, retrieve the format.
        MipsFloatFormat? format = null;
        if (instruction.Type is MipsInstructionType.Float)
            format = ((MipsFloatInstruction)instruction).Format;

        bool hasFormat = instruction.Type is MipsInstructionType.Float;
        bool eretnc = funcCode2 is (byte)Co0FuncCode.ExceptionReturn && instruction.RD is (MipsGpRegister)1;
        var key = new DisassemblerLookup((byte)instruction.OpCode, funcCode, funcCode2, hasFormat || eretnc);
        if (!InstructionTable.TryGetInstruction(key, out var metas, out _, out _, out _))
        {
            return "Unknown instruction";
        }

        // Take the metadata with the most arguments, prefer in-version instructions
        var meta = metas
            .OrderByDescending(x => x.ArgumentPattern.Length)
            .FirstOrDefault();

        Guard.IsNotNull(meta);

        // Apply the format to the name if it exists
        var name = meta.Name;
        if (format.HasValue)
        {
            name = _formatTable.ApplyFormat(name, format.Value);
        }

        StringBuilder pattern = new($"{name} ");
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

    private static string GetRegisterString(MipsGpRegister register, MipsRegisterSet set) => $"${MipsRegisterTable.Instance.GetRegisterString(register, set)}";
}
