// Avishai Dernis 2025

using System.Linq;
using System.Text;
using Zarem.Assembler.Config;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Disassembler.Models;
using Zarem.Disassembler.Models.Instructions;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;

namespace Zarem.Disassembler;

/// <summary>
/// A MIPS disassembler.
/// </summary>
public class MipsDisassembler
{
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
            InstructionType.BasicR => (byte)instruction.FuncCode,
            InstructionType.Special2R => (byte)instruction.Func2Code,
            InstructionType.Special3R => (byte)instruction.Func3Code,
            
            InstructionType.BasicI or
            InstructionType.BasicJ => 0,
            
            InstructionType.RegisterImmediate or
            InstructionType.RegisterImmediateBranch => (byte)instruction.RTFuncCode,

            InstructionType.Coproc0 => (byte)((CoProc0Instruction)instruction).CoProc0RSCode,

            InstructionType.Coproc1 => (byte)((FloatInstruction)instruction).CoProc1RSCode,
            InstructionType.Float => (byte)((FloatInstruction)instruction).FloatFuncCode,

            _ => 255,
        };


        byte funcCode2 = instruction.Type switch
        {
            InstructionType.Coproc0 => funcCode switch
            {
                (byte)CoProc0RSCode.C0 => (byte)((CoProc0Instruction)instruction).Co0FuncCode,
                (byte)CoProc0RSCode.MFMC0 => (byte)((CoProc0Instruction)instruction).MFMC0FuncCode,
                _ => 255,
            },
            _ => 255,
        };

        // If the instruction is a float instruction, retrieve the format.
        FloatFormat? format = null;
        if (instruction.Type is InstructionType.Float)
            format = ((FloatInstruction)instruction).Format;

        bool hasFormat = instruction.Type is InstructionType.Float;
        bool eretnc = funcCode2 is (byte)Co0FuncCode.ExceptionReturn && instruction.RD is (GPRegister)1;
        var key = new DisassemblerLookup((byte)instruction.OpCode, funcCode, funcCode2, hasFormat || eretnc);
        if (!InstructionTable.TryGetInstruction(key, out var metas, out _, out _))
        {
            return "Unknown instruction";
        }

        // Take the metadata with the most arguments, prefer in-version instructions
        var meta = metas
            .OrderByDescending(x => x.ArgumentPattern.Length)
            .FirstOrDefault();

        // Apply the format to the name if it exists
        var name = meta.Name;
        if (format is not null)
        {
            name = FloatFormatTable.ApplyFormat(name, format.Value);
        }

        StringBuilder pattern = new($"{name} ");
        for (int i = 0; i < meta.ArgumentPattern.Length; i++)
        {
            pattern.Append(meta.ArgumentPattern[i] switch
            {
                Argument.RS => RegistersTable.GetRegisterString(instruction.RS),
                Argument.RT => RegistersTable.GetRegisterString(instruction.RT),
                Argument.RD => RegistersTable.GetRegisterString(instruction.RD),
                Argument.Shift => instruction.ShiftAmount,
                Argument.Immediate => instruction.ImmediateValue,
                Argument.Offset => instruction.Offset,
                Argument.LargeOffset => instruction.Address,
                Argument.Address => instruction.Address,
                Argument.AddressBase => $"{instruction.ImmediateValue}({RegistersTable.GetRegisterString(instruction.RS)})",
                Argument.FullImmediate => 0, // Won't happen until pseudo-instruction disassembly
                Argument.FS => RegistersTable.GetRegisterString((GPRegister)((FloatInstruction)instruction).FS, RegisterSet.FloatingPoints),
                Argument.FT => RegistersTable.GetRegisterString((GPRegister)((FloatInstruction)instruction).FT, RegisterSet.FloatingPoints),
                Argument.FD => RegistersTable.GetRegisterString((GPRegister)((FloatInstruction)instruction).FD, RegisterSet.FloatingPoints),
                Argument.RS_Numbered => RegistersTable.GetRegisterString(instruction.RS, RegisterSet.Numbered),
                Argument.RT_Numbered => RegistersTable.GetRegisterString(instruction.RT, RegisterSet.Numbered),
                _ => "unknown",
            });

            if (i < meta.ArgumentPattern.Length - 1)
            {
                pattern.Append(", ");
            }
        }

        return $"{pattern}";
    }
}
