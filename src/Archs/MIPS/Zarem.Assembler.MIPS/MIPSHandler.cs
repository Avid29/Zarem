// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Assembler.Config;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler;

/// <summary>
/// An <see cref="IArchHandler"/> for the mips architecture.
/// </summary>
public class MIPSHandler : IArchHandler
{
    private readonly MIPSAssemblerConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSHandler"/> class.
    /// </summary>
    public MIPSHandler(MIPSAssemblerConfig config)
    {
        _config = config;
    }

    /// <inheritdoc/>
    public string GetArchitectureName() => "MIPS";

    /// <inheritdoc/>
    public int GetInstructionSize(AssemblyLine line)
    {
        // TODO: pseudo-instructions
        return 4;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> GetNOP() => new byte[4];

    /// <inheritdoc/>
    public IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, Logger logger)
    {
        var parser = new MIPSInstructionParser(_config, address, symbols, logger);
        return parser.Parse(line);
    }
}
