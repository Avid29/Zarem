// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Assembler.Logging;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

public interface ICodeEditor
{
    /// <summary>
    /// Gets or sets the text contained in the code editor.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the currently selected line in the code editor.
    /// </summary>
    /// <remarks>
    /// Zero indexed.
    /// </remarks>
    public long Line { get; set; }

    /// <summary>
    /// Gets or sets the currently selected column in the code editor.
    /// </summary>
    /// <remarks>
    /// Zero indexed.
    /// </remarks>
    public long Column { get; set; }

    /// <summary>
    /// Gets or sets the current zoom percentage.
    /// </summary>
    public int Zoom { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the severity threshold to show log annotations below indicators.
    /// </summary>
    public AnnotationThreshold AnnotationThreshold { get; set; }

    /// <summary>
    /// Gets or sets the color scheme used for syntax highlighting in assembly code.
    /// </summary>
    public AssemblySyntaxColorScheme? ColorScheme { get; set;  }

    /// <summary>
    /// Applies formatting based on a log messages.
    /// </summary>
    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs);

    /// <summary>
    /// Clears formatting based on a log messages.
    /// </summary>
    public void ClearLogHighlights();

    void NavigateToToken(SourceLocation location);

    void ResetHistory();
}
