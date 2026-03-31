// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zarem.Assembler.Config;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Parsers.Enums;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models;

namespace Zarem.Assembler.Parsers;

/// <summary>
/// A base class for instruction parsers.
/// </summary>
public abstract class InstructionParserBase<TRegister, TSet>
    where TRegister : unmanaged, Enum
    where TSet : unmanaged, Enum
{
    private readonly RegisterTable<TRegister, TSet> _table;
    private readonly AssemblerLogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterParser{TRegister, TSet}"/> struct.
    /// </summary>
    public InstructionParserBase(RegisterTable<TRegister, TSet> table, ILogger? logger)
    {
        _table = table;

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Gets the assembler configuration.
    /// </summary>
    public abstract AssemblerConfig Config { get; }

    /// <summary>
    /// Attempts to parse a register.
    /// </summary>
    /// <param name="arg">The token to parse as a register.</param>
    /// <param name="register">The resulting register.</param>
    /// <param name="set">The register set the register needs to belong ot.</param>
    /// <param name="bounds">The bounds of a numerical register in the set.</param>
    /// <returns>Whether or not a register was successfuly parsed.</returns>
    public bool TryParseRegister(Token arg, out TRegister register, TSet set, int bounds)
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

    /// <summary>
    /// Cleans a value to a specified bit count and shift amount, while also checking for any changes that occured during the cast.
    /// </summary>
    /// <remarks>
    /// This does not apply the <paramref name="shiftAmount"/>, only masks the lower bits.
    /// </remarks>
    /// <param name="value">A reference to the integer to modify.</param>
    /// <param name="arg">The original tokens of the argument.</param>
    /// <param name="bitCount">The number of bits after casting.</param>
    /// <param name="shiftAmount">The number of bits that will drop from the bottom.</param>
    /// <param name="signed">Whether or not the new value should be signed.</param>
    /// <returns>Whether or not the value can be safely cast.</returns>
    public void CleanInteger(ref long value, ReadOnlySpan<Token> arg, int bitCount, int shiftAmount, bool signed)
    {
        var original = value;

        Guard.IsGreaterThan(bitCount, 1);
        Guard.IsLessThanOrEqualTo(bitCount + shiftAmount, 64);

        // Create a masks for the high and low truncating bits,
        // as well as an overall remaining bits map
        var upperMask = bitCount == 64 ? -1L : (1L << (bitCount + shiftAmount)) - 1;
        var lowerMask = ~((1L << shiftAmount) - 1);
        var mask = upperMask & lowerMask;

        // Truncate mask upper and lower bits
        long truncated = value & mask;

        // Sign extend if signed and not full width
        if (signed && bitCount < 64)
        {
            long signBit = 1L << (bitCount - 1);
            if ((truncated & signBit) != 0)
                truncated |= ~upperMask; // Sign extend
        }

        value = truncated;

        // Compute changes
        var changes = CastingChanges.None;

        // Check if the sign was dropped
        if (!signed && original < 0)
            changes |= CastingChanges.SignChanged;

        // Check for upper truncation
        long upperBits = original & ~upperMask;
        if (upperBits != 0 && upperBits != ~upperMask)
            changes |= CastingChanges.TruncatedHigh;

        // Check for lower truncation
        if ((original & ~lowerMask) != 0)
        {
            changes |= CastingChanges.TruncatedLow;
        }

        // Clean integer to fit within argument bit size and match signs
        // Log a message if the value was truncated and/or had its sign changed
        if (changes is not CastingChanges.None)
        {
            _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg, $"CastWarning{changes}", arg.Print(), original, value, bitCount, shiftAmount);
        }
    }
}
