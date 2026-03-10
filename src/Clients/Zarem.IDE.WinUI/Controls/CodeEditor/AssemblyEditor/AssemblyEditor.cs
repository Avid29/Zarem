// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

/// <summary>
/// A modified <see cref="CodeEditor"/> to add assembly syntax-highlighting and other features.
/// </summary>
public partial class AssemblyEditor : CodeEditor
{
    private const string CodeEditorPartName = "CodeEditorControl";

    /// <remarks>
    /// The text is in UTF8, while the tokenizer output <see cref="SourceLocation"/> is in UTF16.
    /// We make this conversion during syntax highlighting. Track the results for log highlights.
    /// </remarks>
    private readonly Dictionary<int, SourceLocation> _locationMapper;
    private TokenizedAssembly? _tokenizedAssembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyEditor"/> class.
    /// </summary>
    public AssemblyEditor()
    {
        _locationMapper = [];

        DefaultStyleKey = typeof(AssemblyEditor);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Setup events
        this.Loaded += AssemblyEditor_Loaded;

        // Setup keywords
        SetupKeywords();

        // Setup styling
        SetupHighlighting();
        SetupIndicators();
    }

    /// <summary>
    /// Navigates to a <see cref="SourceLocation"/>.
    /// </summary>
    /// <param name="location">The position to navigate to.</param>
    public void NavigateToToken(SourceLocation location)
    {
        // Get the editor
        var editor = ChildEditor?.Editor;
        if (editor is null)
            return;

        // Attempt to get mapped location
        if(!_locationMapper.TryGetValue(location.Index, out var mappedLocation))
            return;

        // Go to position, and focus the keyboard
        editor.EnsureVisible(location.Line - 1);
        editor.GotoPos(mappedLocation.Index);
        ChildEditor?.Focus(FocusState.Keyboard);
    }

    private static int GetEncodingSize(string original)
        => Encoding.UTF8.GetByteCount(original);

    private static int ToInt(Color color) => color.R | color.G << 8 | color.B << 16;
}
