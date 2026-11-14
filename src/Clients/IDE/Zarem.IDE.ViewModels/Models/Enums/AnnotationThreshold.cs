// Avishai Dernis 2025

namespace Zarem.IDE.Services.Settings.Enums;

/// <summary>
/// The threshold to display an annotation.
/// </summary>
public enum AnnotationThreshold
{
    #pragma warning disable CS1591

    Never,
    Errors,
    Warnings,
    Messages,
    Always = Messages, 

    #pragma warning restore CS1591
}
