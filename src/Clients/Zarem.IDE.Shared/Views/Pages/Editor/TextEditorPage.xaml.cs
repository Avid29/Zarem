// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Tokenization.Models;
using Zarem.IDE.Controls.CodeEditor;
using Zarem.IDE.Messages;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Models.EditorConfig.ColorScheme;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.Interfaces;
using Zarem.Models;

namespace Zarem.IDE.Views.Pages.Editor;

public sealed partial class TextEditorPage : UserControl, IFileEditorHandler
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextEditorPage), new PropertyMetadata(string.Empty, OnTextPropertyChanged));

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
            field?.EditorHandler = this;
            _ = LoadContentAsync();
        }
    }

    public CodeEditor ActiveCodeEditor => UseAssemblyEditor ? AssemblyEditor : CodeEditor;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private string? OriginalText
    {
        get => field;
        set
        {
            Text = value ?? string.Empty;
            if (value != field)
            {
                field = value;
                ViewModel?.NotifyStateChanged();
            }
        }
    }

    private bool UseAssemblyEditor => ViewModel?.File?.Name.EndsWith(".asm") ?? false;

    private bool UseTextEditor => !UseAssemblyEditor;

    /// <inheritdoc/>
    public bool IsDirty => Text != OriginalText;

    private void UpdateEvents(FilePageViewModel? newVM, FilePageViewModel? oldVM)
    {
        oldVM?.NavigateToTokenEvent -= ViewModel_NavigateToTokenEvent;
        oldVM?.EditorOperationRequested -= ViewModel_EditorOperationRequested;

        newVM?.NavigateToTokenEvent += ViewModel_NavigateToTokenEvent;
        newVM?.EditorOperationRequested += ViewModel_EditorOperationRequested;
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            var file = ViewModel?.File;
            if (file is null)
                return false;

            await using var stream = await file.FileItem.OpenStreamForWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(Text ?? string.Empty);
            stream.SetLength(stream.Position);
            OriginalText = Text;
        }
        catch
        {
            return false;
        }

        return true;
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

    public static string FormatAddres(Address? address)
    {
        if (!address.HasValue)
        {
            return string.Empty;
        }

        return string.Format($"{address?.Section?.Name}:0x{address?.Offset:X4}");
    }

    public static string GetPositionText(long line, long column)
    {
        var localizationService = Service.Get<ILocalizationService>();
        return localizationService["/Pages/Editor/LineAndColumn", line, column];
    }

    private async Task LoadContentAsync()
    {
        var file = ViewModel?.File;
        var text = string.Empty;

        if (file is not null)
        {
            await using var stream = await file.FileItem.OpenStreamForReadAsync();
            using var reader = new StreamReader(stream);
            text = await reader.ReadToEndAsync();
        }

        OriginalText = text;
    }

    private void ZoomComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        ApplyZoomText(sender, args.Text);
        args.Handled = true;
    }

    private void ApplyZoomText(ComboBox comboBox, string text)
    {
        text = text.Trim().Trim('%').Trim();
        if (int.TryParse(text, out int percent))
        {
            ActiveCodeEditor.Zoom = percent;
        }
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not TextEditorPage page)
            return;

        page.ViewModel?.NotifyStateChanged();
    }
}
