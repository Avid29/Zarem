// Avishai Dernis 2026

namespace Zarem.IDE.Messages.DebugSession;

/// <summary>
/// A message sent when the executing line changes.
/// </summary>
/// <remarks>
/// Only sent if the execution is halted by the debugger, or to clear when resuming.
/// </remarks>
public record ExecutingLineChangedMessage(string? FilePath = null, ulong? LineNumber = null);
