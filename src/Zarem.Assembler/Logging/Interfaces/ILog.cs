// Avishai Dernis 2025

using Zarem.Assembler.Logging.Enum;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Logging.Interfaces;

/// <summary>
/// An <see langword="interface"/> for an entry in the <see cref="ILogger"/>.
/// </summary>
public interface ILog
{
    /// <summary>
    /// Gets the log's severity.
    /// </summary>
    public Severity Severity { get; }

    /// <summary>
    /// Get the log's code.
    /// </summary>
    public LogCode Code { get; }
    
    /// <summary>
    /// Gets the log's message.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Gets the name of the file where the log occurred.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the path of the file where the log occurred.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the location where the log occurred.
    /// </summary>
    public SourceLocation? Location { get; }
}
