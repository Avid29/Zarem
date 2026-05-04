// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Mips.Assembler.Logger;

/// <summary>
/// A logger which simplifies logging events in the assembler.
/// </summary>
internal class AssemblerLogger : LocalLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblerLogger"/> class.
    /// </summary>
    public AssemblerLogger(ILogger parent) : base(parent, "Zarem.Mips.Assembler.Resources.Logger", typeof(AssemblerLogger).Assembly)
    {
    }

    /// <inheritdoc cref="ILogger.Log(ILog)"/>
    public bool Log(Severity severity, LogId id, ReadOnlySpan<Token> tokens, string messageKey, params object?[] args)
    {
        var message = Localizer[messageKey, args];
        Guard.IsNotNull(message);

        return Parent.Log(new AssemblerEntry(severity, new("MPS_ASM", (uint)id), message, [.. tokens]));
    }

    public bool Log(Severity severity, LogId id, Token token, string messageKey, params object?[] args)
        => Log(severity, id, [token], messageKey, args);
}
