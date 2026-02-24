// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.Models.CheatSheet;
using Zarem.WinUI.Controls.CheatSheet.Palettes;

namespace Zarem.WinUI.Controls.CheatSheet;

public sealed partial class EncodingPatternDisplay : UserControl
{
    private static readonly DependencyProperty EncodingPatternProperty =
        DependencyProperty.Register(nameof(EncodingPattern), typeof(EncodingPattern), typeof(EncodingPatternDisplay), new PropertyMetadata(null, EncodingPatternUpdated));

    private static readonly DependencyProperty SectionBrushPaletteProperty =
        DependencyProperty.Register(nameof(SectionBrushPalette), typeof(EncodingPatternSectionBrushPalette), typeof(EncodingPatternDisplay), new PropertyMetadata(null));

    public EncodingPatternDisplay()
    {
        InitializeComponent();
    }

    public EncodingPattern EncodingPattern
    {
        get => (EncodingPattern)GetValue(EncodingPatternProperty);
        set => SetValue(EncodingPatternProperty, value);
    }

    public EncodingPatternSectionBrushPalette SectionBrushPalette
    {
        get => (EncodingPatternSectionBrushPalette)GetValue(SectionBrushPaletteProperty);
        set => SetValue(SectionBrushPaletteProperty, value);
    }

    private static void EncodingPatternUpdated(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not EncodingPatternDisplay epd)
            return;

        epd.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Clear the container contents and column formatting.
        Container.ColumnDefinitions.Clear();
        Container.Children.Clear();

        // Null check the pattern sections
        if (EncodingPattern.Sections is null)
            return;

        for (int i = 0; i < EncodingPattern.Sections.Length; i++)
        {
            var section = EncodingPattern.Sections[i];

            // Create a column definition for the section
            var column = new ColumnDefinition
            {
                Width = new GridLength(section.BitCount, GridUnitType.Star)
            };
            Container.ColumnDefinitions.Add(column);

            // Create content control for the section
            var content = new ContentControl
            {
                Content = section,
                ContentTemplateSelector = EncodingSectionTemplateSelector,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetColumn(content, i);
            Container.Children.Add(content);

        }
    }
}
