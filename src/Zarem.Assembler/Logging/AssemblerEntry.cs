// Avishai Dernis 2024

using System.IO;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Assembler.Logging;

/// <summary>
/// An <see cref="ILog"/> that occurred in the assembler.
/// </summary>
public class AssemblerEntry : ILog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblerEntry"/> class.
    /// </summary>
    public AssemblerEntry(Severity severity, LogCode code, string message, Token[] tokens)
    {
        Code = code;
        Severity = severity;
        Message = message;
        Tokens = tokens;
        FilePath = tokens[0].FilePath;
    }

    /// <inheritdoc/>
    public LogCode Code { get; }
    
    /// <inheritdoc/>
    public Severity Severity { get; }
    
    /// <inheritdoc/>
    public string Message { get; }
    
    /// <inheritdoc/>
    public string? FileName => Path.GetFileName(FilePath);
    
    /// <inheritdoc/>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the tokens that caused the log.
    /// </summary>
    public Token[] Tokens { get; }

    /// <inheritdoc/>
    public SourceLocation? Location
    {
        get
        {
            if (Tokens.Length is 0)
                return null;

            return Tokens[0].Location;
        }
    }
}
