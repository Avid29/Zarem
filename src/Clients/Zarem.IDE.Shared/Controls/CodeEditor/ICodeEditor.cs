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
    /// Gets or sets the color scheme used for syntax highlighting in assembly code.
    /// </summary>
    public AssemblySyntaxColorScheme? ColorScheme { get; set;  }

    void NavigateToToken(SourceLocation location);

    void ResetHistory();
}
