// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Components.Interfaces;
using Zarem.Helpers;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Models;
using Zarem.Models.Breakpoints;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

[TemplatePart(Name = ICodeEditorPartName, Type = typeof(ICodeEditor))]
public sealed partial class CodeEditor : Control, ICodeEditor
{
    private const int ThrottleThresholdMs = 250;
    private readonly Stopwatch _throttleStopwatch = Stopwatch.StartNew();
    private TokenizedAssembly? _tokenizedAssembly;
    private bool _isAssemblerQueued;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CodeEditor), new PropertyMetadata(string.Empty, OnTextPropertyChanged));

    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register(nameof(Line), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(long), typeof(CodeEditor), new PropertyMetadata(0L, OnPositionPropertyChanged));

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(int), typeof(CodeEditor), new PropertyMetadata(100));

    public static readonly DependencyProperty ColorSchemeProperty =
        DependencyProperty.Register(nameof(ColorScheme), typeof(AssemblySyntaxColorScheme), typeof(CodeEditor), new PropertyMetadata(null));

    public static readonly DependencyProperty RealTimeAssemblyProperty
        = DependencyProperty.Register(nameof(RealTimeAssembly), typeof(bool), typeof(CodeEditor), new PropertyMetadata(true, OnRealTimeAssemblyPropertyChanged));

    public static readonly DependencyProperty AnnotationThresholdProperty =
        DependencyProperty.Register(nameof(AnnotationThreshold), typeof(AnnotationThreshold), typeof(CodeEditor), new PropertyMetadata(AnnotationThreshold.Errors, OnLogAnnotationsChanged));

    public static readonly DependencyProperty PositionAddressProperty =
        DependencyProperty.Register(nameof(PositionAddress), typeof(Address?),typeof(CodeEditor), new PropertyMetadata(null));

    public static readonly DependencyProperty ExecutingLocationProperty =
        DependencyProperty.Register(nameof(ExecutingLocation), typeof(SourceRange?), typeof(CodeEditor), new PropertyMetadata(null));

    private const string ICodeEditorPartName = "PART_ICodeEditor";

    private ICodeEditor? _codeEditor;

    public CodeEditor()
    {
        this.DefaultStyleKey = typeof(CodeEditor);
    }

    protected override void OnApplyTemplate()
    {
        _codeEditor = (ICodeEditor)GetTemplateChild(ICodeEditorPartName);
    }

    /// <inheritdoc/>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <inheritdoc/>
    public long Line
    {
        get => (long)GetValue(LineProperty);
        set => SetValue(LineProperty, value);
    }

    /// <inheritdoc/>
    public long Column
    {
        get => (long)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    /// <inheritdoc/>
    public int Zoom
    {
        get => (int)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <inheritdoc/>
    public AssemblySyntaxColorScheme? ColorScheme
    {
        get => (AssemblySyntaxColorScheme?)GetValue(ColorSchemeProperty);
        set => SetValue(ColorSchemeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not to check assembly errors in real-time.
    /// </summary>
    public bool RealTimeAssembly
    {
        get => (bool)GetValue(RealTimeAssemblyProperty);
        set => SetValue(RealTimeAssemblyProperty, value);
    }

    /// <inheritdoc/>
    public AnnotationThreshold AnnotationThreshold
    {
        get => (AnnotationThreshold)GetValue(AnnotationThresholdProperty);
        set => SetValue(AnnotationThresholdProperty, value);
    }

    public Address? PositionAddress
    {
        get => (Address?)GetValue(PositionAddressProperty);
        set => SetValue(PositionAddressProperty, value);
    }

    public SourceRange? ExecutingLocation
    {
        get => (SourceRange?)GetValue(ExecutingLocationProperty);
        set => SetValue(ExecutingLocationProperty, value);
    }

    public IAssembleComponent? Assembler { get; set; }

    public SymbolResolver? SymbolResolver { get; private set; }

    public void NavigateToToken(SourceLocation location) => _codeEditor?.NavigateToToken(location);

    public void ResetHistory() => _codeEditor?.ResetHistory();

    /// <inheritdoc/>
    public void ApplyOperation(EditorOperation operation) => _codeEditor?.ApplyOperation(operation);

    /// <inheritdoc/>
    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs) => _codeEditor?.ApplyLogHighlights(logs);

    /// <inheritdoc/>
    public void ClearLogHighlights() => _codeEditor?.ClearLogHighlights();

    public void RegisterBreakpointSource(BreakpointCollection breakpoints) => _codeEditor?.RegisterBreakpointSource(breakpoints);

    public void UnregisterBreakpointSource() => _codeEditor?.UnregisterBreakpointSource();

    public static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.RequestThrottledAssembly();
    }

    private static void OnRealTimeAssemblyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.ClearLogHighlights();
        _ = codeEditor.RunAssemblerAsync();
    }

    private static void OnLogAnnotationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not CodeEditor codeEditor)
            return;

        // TODO: This is dirty. Change this
        codeEditor._codeEditor?.AnnotationThreshold = codeEditor.AnnotationThreshold;

        codeEditor.ClearLogHighlights();
        _ = codeEditor.RunAssemblerAsync();
    }

    private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not CodeEditor codeEditor)
            return;

        codeEditor.UpdatePositionAddress();
    }

    private async Task RunAssemblerAsync()
    {
        _throttleStopwatch.Restart();

        if (!RealTimeAssembly || Assembler is null)
            return;

        // Run assembler and show errors
        try
        {
            var result = await Zarembler.AssembleAsync(Text, "editor", Assembler.Handler, Assembler.Config);
            SymbolResolver = new SymbolResolver(result.Symbols);
            _tokenizedAssembly = result.Tokens;

            ApplyLogHighlights(result.Logs);
        }
        catch (Exception)
        {
            // TODO: Notify exception occured
        }
    }

    private void UpdatePositionAddress()
    {
        if (_tokenizedAssembly is null || _tokenizedAssembly.LineCount is 0)
            return;

        var line = Math.Min(Line, _tokenizedAssembly.LineCount - 1);
        var asmLine = _tokenizedAssembly[(int)line];
        PositionAddress = asmLine.Address;
    }

    private async void RequestThrottledAssembly()
    {
        // If a run is already queued to happen, don't stack more
        if (_isAssemblerQueued)
            return;

        long elapsed = _throttleStopwatch.ElapsedMilliseconds;

        if (elapsed >= ThrottleThresholdMs)
        {
            // It's been long enough, run immediately
            await RunAssemblerAsync();
        }
        else
        {
            // Too soon. Queue a run for the remaining time
            _isAssemblerQueued = true;
            await Task.Delay((int)(ThrottleThresholdMs - elapsed));
            await RunAssemblerAsync();
            _isAssemblerQueued = false;
        }
    }
}
