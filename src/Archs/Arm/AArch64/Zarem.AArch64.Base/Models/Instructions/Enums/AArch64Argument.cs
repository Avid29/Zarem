// Avishai Dernis 2026

using System.Text.Json.Serialization;

namespace Zarem.AArch64.Models.Instructions.Enums;

/// <summary>
/// An enum for AArch64 argument types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AArch64Argument
{
}
