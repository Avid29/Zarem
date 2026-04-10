// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler;

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
    public int GetInstructionSize(AssemblyLine line) => 4;

    /// <inheritdoc/>
    public ReadOnlySpan<byte> GetNOP() => [0x13, 0x00, 0x00, 0x00]; // addi x0, x0, 0

    /// <inheritdoc/>
    public IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, ILogger? logger)
    {
        var parser = new RiscVInstructionParser(Config, _instructionTable, address, symbols, logger);
        return parser.Parse(line);
    }
}
