// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Logger;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Tables;

namespace Zarem.Assembler;


/// <summary>
/// A struct for parsing RISC-V instructions.
/// </summary>
public struct RiscVInstructionParser
{
    private readonly RiscVAssemblerConfig? _config;
    private readonly Address _currentAddress;
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    private RiscVInstructionMetaBase? _meta;

    //private GPRegister _rd;
    //private GPRegister _rs1;
    //private GPRegister _rs2;
    //private int _immediate;
    private List<RelocationEntry>? _references;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionParser"/> struct.
    /// </summary>
    public RiscVInstructionParser(RiscVAssemblerConfig config, RiscVInstructionTable? table, Address address, IReadOnlyDictionary<string, Symbol>? symbols, ILogger? logger)
    {
        _config = config;
        _currentAddress = address;
        _symbols = symbols;
        _references = [];

        _instructionTable = table ?? new RiscVInstructionTable(config);

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <returns>The parser instruction.</returns>
    public void Parse(AssemblyLine line)
    {
        // Attempt to load the instruction
        // If successful, this will set the _meta and _format
        if (!TryParseInstruction(line, out var name))
            return;

        // Parse argument data according to pattern
        Argument[] pattern = _meta.ArgumentPattern;
        for (int i = 0; i < line.Args.Count; i++)
        {
            // Split out next arg
            var arg = line.Args[i];

            // Empty argument
            if (arg.Tokens.Length is 0)
            {
                var reportToken = arg.ProceedingComma ?? arg.PrecedingComma;
                Guard.IsNotNull(reportToken);
                _logger?.Log(Severity.Error, LogId.InvalidInstructionArg, reportToken, "EmptyArgument");
                continue;
            }

            //TryParseArg(arg.Tokens, pattern[i]);
        }
    }

    [MemberNotNullWhen(true, nameof(_meta))]
    private bool TryParseInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var requiredBase, out var requiredExtension))
        {
            (LogId id, string message) = requiredExtension switch
            {
                not null => (LogId.NotInVersion, "RequiresExtension"),
                null when requiredBase is not null => (LogId.NotInVersion, "RequiresVersion"),
                null => (LogId.InvalidInstructionName, "NoInstructionNamed")
            };

            _logger?.Log(Severity.Error, id, line.Instruction, message, name, $"{requiredBase:d}", $"{requiredExtension:d}");
            return false;
        }

        _meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        if (_meta is null)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        return true;
    }

    //private bool TryParseArg(ReadOnlySpan<Token> arg, Argument type)
    //{
    //    return type switch
    //    {
    //        // Register arguments
    //        (>= Argument.RD and <= Argument.RS2) or 
    //        (>= Argument.FRD and <= Argument.FRS3) => TryParseRegisterArg(arg, type),

    //        // Expression arguments
    //        Argument.ShiftAmount or Argument.Immediate or Argument.FullImmediate
    //        or Argument.Offset or Argument.LargeOffset or Argument.Address => TryParseExpressionArg(arg, type),

    //        // Address offset arguments
    //        Argument.AddressBase => TryParseAddressOffsetArg(arg),

    //        _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
    //    };
    //}
}
