// Avishai Dernis 2026

using System.Xml.Serialization;

namespace Zarem.Emulator.Config.Enums;

/// <summary>
/// An enum for the execution mode.
/// </summary>
public enum ExecutionMode
{
#pragma warning disable CS1591

    [XmlEnum("interpret")] Interpret,
    [XmlEnum("jit")] JustInTime,

#pragma warning restore CS1591
}
