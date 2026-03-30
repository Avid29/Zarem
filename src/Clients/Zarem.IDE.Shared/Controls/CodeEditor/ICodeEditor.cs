// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Logging;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Models.Breakpoints;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

public interface ICodeEditor
{
    /// <summary>
    /// Gets or sets the text contained in the code editor.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Gets or sets the currently selected line in the code editor.
    /// </summary>
    /// <remarks>
    /// Zero indexed.
    /// </remarks>
    long Line { get; set; }

    /// <summary>
    /// Gets or sets the currently selected column in the code editor.
    /// </summary>
    /// <remarks>
    /// Zero indexed.
    /// </remarks>
    long Column { get; set; }

    /// <summary>
    /// Gets or sets the current zoom percentage.
    /// </summary>
    int Zoom { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the severity threshold to show log annotations below indicators.
    /// </summary>
    AnnotationThreshold AnnotationThreshold { get; set; }

    /// <summary>
    /// Gets or sets the color scheme used for syntax highlighting in assembly code.
    /// </summary>
    AssemblySyntaxColorScheme? ColorScheme { get; set;  }

    SourceRange? ExecutingLocation { get; set; }

    void ApplyOperation(EditorOperation operation);

    /// <summary>
    /// Applies formatting based on a log messages.
    /// </summary>
    void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs);

    /// <summary>
    /// Clears formatting based on a log messages.
    /// </summary>
    void ClearLogHighlights();

    void RegisterBreakpointSource(BreakpointCollection breakpoints);

    void UnregisterBreakpointSource();

    void NavigateToToken(SourceLocation location);

    void ResetHistory();
}
