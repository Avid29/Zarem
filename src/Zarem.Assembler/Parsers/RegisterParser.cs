// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// A struct for parsing register arguments in assembly instructions.
/// </summary>
public readonly struct RegisterParser<TRegister, TSet>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
{
    private readonly RegisterTable<TRegister, TSet> _table;
    private readonly AssemblerLogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterParser{TRegister, TSet}"/> struct.
    /// </summary>
    public RegisterParser(RegisterTable<TRegister, TSet> table, ILogger? logger)
    {
        _table = table;

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Attempts to parse a register.
    /// </summary>
    /// <param name="arg">The token to parse as a register.</param>
    /// <param name="register">The resulting register.</param>
    /// <param name="set">The register set the register needs to belong ot.</param>
    /// <param name="bounds">The bounds of a numerical register in the set.</param>
    /// <returns>Whether or not a register was successfuly parsed.</returns>
    public readonly bool TryParseRegister(Token arg, out TRegister register, TSet set, int bounds)
    {
        register = default;

        // Check that argument is register argument
        if (arg.Type is not TokenType.Register)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "ArgumentNotARegister", arg);
            return false;
        }

        // Get named register from table
        if (!_table.TryGetRegister(arg.Source, out register, out TSet parsedSet, out bool indexed))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "RegisterNotFound", arg);
            return false;
        }

        var index = Unsafe.As<TRegister, int>(ref register);
        if (index >= bounds)
        {
            var (message, msgArg) = indexed switch
            {
                true => ("RegisterNumberNotFound", (object)index),
                false => ("RegisterNotIndexable", arg)
            };

            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, message, msgArg);
            return false;
        }

        // Match register set
        if (!EqualityComparer<TSet>.Default.Equals(parsedSet, default) && !EqualityComparer<TSet>.Default.Equals(parsedSet, set))
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, $"RegisterWrongSet", arg);
            return false;
        }

        return true;
    }
}
