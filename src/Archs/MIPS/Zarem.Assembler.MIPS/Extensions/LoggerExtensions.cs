// Avishai Dernis 2026

using System;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Extensions;

internal static class LoggerExtensions
{
    public static bool Log(this ILogger logger, Severity severity, LogId id, ReadOnlySpan<Token> tokens, string messageKey, params object?[] args)
        => logger.Log(severity, new LogCode("MPS_ASM", (uint)id), tokens, messageKey, args);

    public static bool Log(this ILogger logger, Severity severity, LogId id, Token token, string messageKey, params object?[] args)
        => logger.Log(severity, new LogCode("MPS_ASM", (uint)id), [token], messageKey, args);
}
