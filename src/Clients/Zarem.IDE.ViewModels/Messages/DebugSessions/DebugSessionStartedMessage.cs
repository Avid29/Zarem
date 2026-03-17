// Avishai Dernis 2026

using Zarem.DebugSessions;

namespace Zarem.IDE.Messages.DebugSessions;

/// <summary>
/// A message sent when a debug session begins.
/// </summary>
public record DebugSessionStartedMessage(DebugSession Session);
