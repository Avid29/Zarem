// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Threading.Tasks;
using Zarem.IDE.Controls.CodeEditor;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.Interfaces;
using Zarem.Models;
using Zarem.Models.Tables;

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

        //Service.Get<IMessenger>().Register<TextEditorPage, SettingChangedMessage<Theme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());
        //Service.Get<IMessenger>().Register<TextEditorPage, SettingChangedMessage<EditorColorScheme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());

        Loaded += TextEditorPage_Loaded;
    }

    private async void TextEditorPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextEditorPage page)
            return;

        page.Loaded -= TextEditorPage_Loaded;

        await page.LoadContentAsync();
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
        => CodeEditor?.NavigateToToken(e);

    private void ViewModel_EditorOperationRequested(object? sender, EditorOperation e)
    {
        //// Find editbox
        //var codeEditor = this.FindDescendant<CodeEditor>();
        //if (codeEditor is null)
        //    return;

        //codeEditor.ApplyOperation(e);
    }

    public string FormatAddres(Address? address)
    {
        return string.Empty;

        //    if (AssemblyEditor is null || !address.HasValue)
        //        return string.Empty;

        //    var addr = address.Value;

        //    // Phrase the location in terms of the section
        //    string sectionOffsetStr = $"{address?.Section?.Name}:0x{addr.Offset:X8}";

        //    // Attempt to phrase location in terms of a symbol
        //    string? symbolOffsetStr = null;
        //    var symbol = AssemblyEditor.SymbolResolver?.FindNearest(addr, out _);
        //    if (symbol is not null)
        //    {
        //        var symOffset = addr.Offset - symbol.Address.Offset;
        //        symbolOffsetStr = $"{symbol.Name}+0x{symOffset:X4}";
        //    }

        //    // Format the string based on available info
        //    if (symbolOffsetStr is not null)
        //    {
        //        return $"{symbolOffsetStr} ({sectionOffsetStr})";
        //    }
        //    else
        //    {
        //        return sectionOffsetStr;
        //    }
    }

    public static string GetPositionText(long line, long column)
    {
        var localizationService = Service.Get<ILocalizationService>();
        return localizationService["/Pages/TextEditor/LineAndColumn", line + 1, column + 1];
    }

    private async Task LoadContentAsync()
    {
        // Defer until loaded
        if (CodeEditor is null)
            return;

        var file = ViewModel?.File;
        var text = string.Empty;

        if (file is not null)
        {
            await using var stream = await file.FileItem.OpenStreamForReadAsync();
            using var reader = new StreamReader(stream);
            text = await reader.ReadToEndAsync();

            //if (UseAssemblyEditor && file.SourceFile is not null)
            //{
            //    AssemblyEditor?.RegisterBreakpointSource(file.SourceFile.Breakpoints);
            //}
        }

        OriginalText = text;
        CodeEditor?.ResetHistory();
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
            CodeEditor?.Zoom = percent;
        }
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not TextEditorPage page)
            return;

        page.ViewModel?.NotifyStateChanged();
    }
}
