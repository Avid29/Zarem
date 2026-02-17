// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;

namespace Zarem.Linker.Extensions;

internal static class LoggerExtensions
{
    public static bool Log(this ILogger logger, Severity severity, LogId id, string filePath, string messageKey, params object?[] args)
        => logger.Log(severity, new LogCode("LNK", (uint)id), filePath, messageKey, args);
}
