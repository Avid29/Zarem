// Adam Dernis 2024

using System;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Localization;

namespace Zarem.Assembler.Logging.Interfaces;

/// <summary>
/// An interface for the <see cref="Logger"/> that only allows creating logs, not reading or managing logs.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Registers a string source with the logger.
    /// </summary>
    void Register(IStringLocalizer localizer);

    /// <summary>
    /// Creates a new log.
    /// </summary>
    /// <remarks>
    /// Logged as a linker log.
    /// </remarks>
    /// <param name="severity">The severity of the log.</param>
    /// <param name="code">The id of the log.</param>
    /// <param name="file">The file where the log occurred.</param>
    /// <param name="messageKey">The log resource key for the log message.</param>
    /// <param name="args">The arguments to format the message with.</param>
    /// <returns>False if the severity is an error. True otherwise.</returns>
    bool Log(Severity severity, LogCode code, string file, string messageKey, params object?[] args);

    /// <inheritdoc cref="Log(Severity, LogCode, ReadOnlySpan{Token}, string, object?[])"/>
    bool Log(Severity severity, LogCode code, Token token, string messageKey, params object?[] args);

    /// <summary>
    /// Creates a new log.
    /// </summary>
    /// <remarks>
    /// Logged as an assembler log.
    /// </remarks>
    /// <param name="severity">The severity of the log.</param>
    /// <param name="code">The id of the log.</param>
    /// <param name="tokens">The token(s) where the log occurred.</param>
    /// <param name="messageKey">The log resource key for the log message.</param>
    /// <param name="args">The arguments to format the message with.</param>
    bool Log(Severity severity, LogCode code, ReadOnlySpan<Token> tokens, string messageKey, params object?[] args);

    /// <summary>
    /// Flushes the current log status.
    /// </summary>
    void Flush();
}
