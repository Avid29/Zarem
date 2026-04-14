// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Models;
using Zarem.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.Assembler;

/// <summary>
/// An <see cref="IAssemblerHandler"/> for the MIPS architecture.
/// </summary>
public class MipsAssemblerHandler : IAssemblerHandler<MipsAssemblerConfig>
{
    private readonly MipsInstructionTable _instructionTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsAssemblerHandler"/> class.
    /// </summary>
    public MipsAssemblerHandler(MipsAssemblerConfig config)
    {
        _instructionTable = new(config);
        Config = config;
    }

    /// <inheritdoc/>
    public string GetArchitectureName() => "MIPS";

    /// <inheritdoc/>
    public MipsAssemblerConfig Config { get; }

    /// <inheritdoc/>
    public Endianness Endianness => Endianness.Big;

    /// <inheritdoc/>
    public ITokenizerProfile TokenizerProfile => MipsTokenizerProfile.Default;

    /// <inheritdoc/>
    public int GetInstructionSize(AssemblyLine line)
    {
        Guard.IsNotNull(_instructionTable);
        Guard.IsNotNull(line.Instruction);

        if (_instructionTable.TryGetInstruction(line.Instruction.Source, line.Args.Count, out var meta, out _, out _, out _))
        {
            var count = (meta as PseudoInstructionMeta)?.RealizedCount ?? 1;
            return count * 4;
        }

        // Instruction not found.
        // Add a nop and less the second pass handle the error
        return 4;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> GetNOP() => new byte[4];

    /// <inheritdoc/>
    public IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, ILogger? logger)
    {
        var parser = new MipsInstructionParser(Config, _instructionTable, address, symbols, logger);
        return parser.Parse(line);
    }
}
