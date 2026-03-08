// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Linker.Logging;

/// <summary>
/// An <see cref="ILog"/> that occurred in the linker.
/// </summary>
public class LinkerLogEntry : ILog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkerLogEntry"/> class.
    /// </summary>
    public LinkerLogEntry(Severity severity, LogCode code, string message, string? module)
    {
        Severity = severity;
        Code = code;
        Message = message;
        FilePath = module;
    }

    /// <inheritdoc/>
    public Severity Severity { get; }

    /// <inheritdoc/>
    public LogCode Code { get; }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string? FileName { get; }

    /// <inheritdoc/>
    public string? FilePath { get; }

    /// <inheritdoc/>
    public SourceLocation? Location { get; }
}
