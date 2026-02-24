// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Messages;
using Zarem.Messages.Editor.Enums;
using Zarem.Models.EditorConfig.ColorScheme;
using Zarem.Services;
using Zarem.Services.Settings.Enums;
using Zarem.ViewModels.Pages;
using Zarem.WinUI.Controls.CodeEditor;

namespace Zarem.WinUI.Views.Pages.Editor;

public sealed partial class TextEditorPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextEditorPage"/> class.
    /// </summary>
    public TextEditorPage()
    {
        InitializeComponent();

        Service.Get<IMessenger>().Register<TextEditorPage, SettingChangedMessage<Theme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());
        Service.Get<IMessenger>().Register<TextEditorPage, SettingChangedMessage<EditorColorScheme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());
    }

    /// <summary>
    /// Gets or sets the <see cref="FilePageViewModel"/>.
    /// </summary>
    public FilePageViewModel? ViewModel
    {
        get;
        set
        {
            UpdateEvents(value, ViewModel);
            field = value;
            UpdateBindings();
        }
    }

    private bool UseAssemblyEditor => ViewModel?.File?.Name.EndsWith(".asm") ?? false;

    private bool UseTextEditor => !UseAssemblyEditor;

    private void UpdateEvents(FilePageViewModel? newVM, FilePageViewModel? oldVM)
    {
        if (oldVM is not null)
        {
            oldVM.NavigateToTokenEvent -= ViewModel_NavigateToTokenEvent;
            oldVM.EditorOperationRequested -= ViewModel_EditorOperationRequested;
        }

        if (newVM is not null)
        {
            newVM.NavigateToTokenEvent += ViewModel_NavigateToTokenEvent;
            newVM.EditorOperationRequested += ViewModel_EditorOperationRequested;
        }
    }

    private void ViewModel_NavigateToTokenEvent(object? sender, SourceLocation e)
    {
        // Find editbox
        var asmEditor = this.FindDescendant<AssemblyEditor>();
        if (asmEditor is null)
            return;

        // Navigate to location
        asmEditor.NavigateToToken(e);
    }

    private void ViewModel_EditorOperationRequested(object? sender, EditorOperation e)
    {
        // Find editbox
        var codeEditor = this.FindDescendant<CodeEditor>();
        if (codeEditor is null)
            return;

        codeEditor.ApplyOperation(e);
    }

    private void UpdateBindings()
    {
        this.Bindings.Update();
    }
}
