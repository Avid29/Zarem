// Avishai Dernis 2025

namespace Zarem.Messages.Editor.Enums;

/// <summary>
/// An enum for editor opertion requests.
/// </summary>
public enum EditorOperation
{
#pragma warning disable CS1591
    // Basic
    Undo,
    Redo,
    Cut,
    Copy,
    Paste,
    Duplicate,
    SelectAll,

    // Advanced
    TransposeUp,
    TransposeDown,

    // Outlining
    ToggleOutlining,
    ExpandChildren,
    CollapseChildren,
    ExpandAll,
    CollapseAll,

#pragma warning restore CS1591
}
