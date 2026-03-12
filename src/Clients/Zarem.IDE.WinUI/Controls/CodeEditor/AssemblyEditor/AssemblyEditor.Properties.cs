// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Zarem.Assembler.Config;
using Zarem.Assembler.Models;
using Zarem.Helpers;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.Models;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private SymbolResolver? _symbolResolver;

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="RealTimeAssembly"/> property.
    /// </summary>
    public static readonly DependencyProperty RealTimeAssemblyProperty =
        DependencyProperty.Register(nameof(RealTimeAssembly),
            typeof(bool),
            typeof(AssemblyEditor),
            new PropertyMetadata(true, OnRTAssemblyChanged));

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="RealTimeAssembly"/> property.
    /// </summary>
    public static readonly DependencyProperty AnnotationThresholdProperty =
        DependencyProperty.Register(nameof(AnnotationThreshold),
            typeof(AnnotationThreshold),
            typeof(AssemblyEditor),
            new PropertyMetadata(AnnotationThreshold.Errors, OnLogAnnotationsChanged));

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="SyntaxHighlightingTheme"/> property.
    /// </summary>
    public static readonly DependencyProperty SyntaxHighlightingThemeProperty =
        DependencyProperty.Register(
            nameof(SyntaxHighlightingTheme),
            typeof(AssemblySyntaxHighlightingTheme),
            typeof(AssemblyEditor),
            new PropertyMetadata(new AssemblySyntaxHighlightingTheme(), OnSyntaxHighlightingThemeChanged));

    /// <summary>
    /// A <see cref="DependencyProperty"/> for the <see cref="AssemblerConfig"/> property.
    /// </summary>
    public static readonly DependencyProperty AssemblerConfigProperty =
        DependencyProperty.Register(
            nameof(AssemblerConfig),
            typeof(AssemblerConfig),
            typeof(AssemblyEditor),
            new PropertyMetadata(default(AssemblerConfig), OnAssemblerConfigChanged));

    public static readonly DependencyProperty PositionAddressProperty =
        DependencyProperty.Register(
            nameof(PositionAddress),
            typeof(Address?),
            typeof(AssemblyEditor),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets a value indicating whether or not to check assembly errors in real-time.
    /// </summary>
    public bool RealTimeAssembly
    {
        get => (bool)GetValue(RealTimeAssemblyProperty);
        set => SetValue(RealTimeAssemblyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not to show log annotations below indicators.
    /// </summary>
    public AnnotationThreshold AnnotationThreshold
    {
        get => (AnnotationThreshold)GetValue(AnnotationThresholdProperty);
        set => SetValue(AnnotationThresholdProperty, value);
    }

    /// <summary>
    /// Gets or sets the text contained in the editbox.
    /// </summary>
    public AssemblySyntaxHighlightingTheme SyntaxHighlightingTheme
    {
        get => (AssemblySyntaxHighlightingTheme)GetValue(SyntaxHighlightingThemeProperty);
        set => SetValue(SyntaxHighlightingThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the assembler configuration to use for <see cref="RealTimeAssembly"/> checks.
    /// </summary>
    public MipsAssemblerConfig? AssemblerConfig
    {
        get => (MipsAssemblerConfig)GetValue(AssemblerConfigProperty);
        set => SetValue(AssemblerConfigProperty, value);
    }

    public Address? PositionAddress
    {
        get => (Address?)GetValue(PositionAddressProperty);
        set => SetValue(PositionAddressProperty, value);
    }

    /// <summary>
    /// Gets the assembler result from the last real-time assembly run.
    /// </summary>
    public AssemblerResult? AssemblerResult
    {
        get => field;
        set
        {
            field = value;
            _symbolResolver = null;
        }
    }

    
    /// <summary>
    /// Gets the symbol resolver for the assembler context.
    /// </summary>
    public SymbolResolver? SymbolResolver
    {
        get
        {
            // No symbols to resolve
            if (AssemblerResult is null)
                return null;

            // Return existing symbol resolver, or create if needed
            return _symbolResolver ??= new(AssemblerResult.Symbols);
        }
    }

    private static void OnSyntaxHighlightingThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not AssemblyEditor asmBox)
            return;

        // Local handler to update colors
        void UpdateHandled(object? sender, EventArgs e) => asmBox.OnColorSchemeUpdated();

        // Unsubscribe from old value
        if (arg.OldValue is AssemblySyntaxHighlightingTheme old)
            old.Updated -= UpdateHandled;

        // Apply new color scheme and subscribe to updates
        asmBox.OnColorSchemeUpdated();
        asmBox.SyntaxHighlightingTheme.Updated += UpdateHandled;

    }

    private void OnColorSchemeUpdated()
    {
        // This is not great
        Background = new SolidColorBrush(SyntaxHighlightingTheme.BackgroundColor);

        SetupHighlighting();
        SetupIndicators();
        UpdateSyntaxHighlighting();
    }

    private static void OnRTAssemblyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
    {
        if (d is not AssemblyEditor asmBox)
            return;

        asmBox.ClearLogHighlights();
        _ = asmBox.RunAssemblerAsync();
    }
    
    private static void OnLogAnnotationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not AssemblyEditor asmBox)
            return;

        asmBox.ClearLogHighlights();
        _ = asmBox.RunAssemblerAsync();
    }

    private static void OnAssemblerConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
        if (d is not AssemblyEditor asmBox)
            return;

        asmBox.SetupKeywords();
    }
}
