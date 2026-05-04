// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Zarem.Assembler;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Models;
using Zarem.Models.Enums;
using Zarem.Models.Tables;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Models.Instructions;

namespace Zarem.RiscV.Assembler;

/// <summary>
/// An <see cref="IAssemblerHandler"/> for the RISC-V architecture.
/// </summary>
public class RiscVAssemblerHandler : IAssemblerHandler<RiscVAssemblerConfig>
{
    private readonly RiscVInstructionTable _instructionTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVAssemblerHandler"/> class.
    /// </summary>
    public RiscVAssemblerHandler(RiscVAssemblerConfig config)
    {
        _instructionTable = new(config);
        Config = config;
    }

    /// <inheritdoc/>
    public RiscVAssemblerConfig Config { get; }

    /// <inheritdoc/>
    public ITokenizerProfile TokenizerProfile => new RiscVTokenizerProfile();

    /// <inheritdoc/>
    public string GetArchitectureName() => "RISC-V";

    /// <inheritdoc/>
    public Endianness Endianness => Endianness.Little;

    /// <inheritdoc/>
    public int GetInstructionSize(AssemblyLine line)
    {
        Guard.IsNotNull(_instructionTable);
        Guard.IsNotNull(line.Instruction);

        if (_instructionTable.TryGetInstruction(line.Instruction.Source, line.Args.Count, out var meta, out _, out _))
        {
            var count = (meta as IPseudoInstructionMeta)?.Expansion.Length ?? 1;
            return count * 4;
        }

        // Instruction not found.
        // Add a nop and less the second pass handle the error
        return 4;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> GetNOP() => [0x13, 0x00, 0x00, 0x00]; // addi x0, x0, 0

    /// <inheritdoc/>
    public IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, ILogger? logger)
    {
        var parser = new RiscVInstructionParser(Config, _instructionTable, address, symbols, logger);
        var instructions = parser.Parse(line, out var references);
        if (instructions is null)
            return null;

        return new ParsedInstructionBase<RiscVInstruction>(instructions, references) { Endianness = Endianness };
    }
}
