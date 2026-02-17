// Adam Dernis 2024

using System.Collections.Generic;

namespace Zarem.Assembler.Logging.Interfaces;

/// <summary>
/// An interface for the <see cref="Logger"/> that only allows creating logs, not reading or managing logs.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an event
    /// </summary>
    public bool Log(ILog log);

    /// <summary>
    /// Flushes the current log status.
    /// </summary>
    void Flush();

    /// <summary>
    /// Gets a value indicating whether or not assembly failed.
    /// </summary>
    bool CurrentFailed { get; }

    /// <summary>
    /// Gets a readonly list of logs for the current file.
    /// </summary>
    IReadOnlyList<ILog> CurrentLog { get; }

    /// <summary>
    /// Gets a readonly list of logs.
    /// </summary>
    IEnumerable<ILog> Logs { get; }
}
