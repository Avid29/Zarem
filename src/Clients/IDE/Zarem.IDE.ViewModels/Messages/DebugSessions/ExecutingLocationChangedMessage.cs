// Avishai Dernis 2026

using Zarem.Models.Tables;

namespace Zarem.IDE.Messages.DebugSessions;

/// <summary>
/// A message sent when the executing line changes.
/// </summary>
/// <remarks>
/// Only sent if the execution is halted by the debugger, or to clear when resuming.
/// </remarks>
public record ExecutingLocationChangedMessage(SourceRange? Location = null);
