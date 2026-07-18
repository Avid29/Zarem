// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;

namespace Zarem.IDE.Controls.CheatSheet;

public sealed partial class EncodingTableDisplay : UserControl
{
    private object[]? _cellData;

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(EncodingTableDisplay), new PropertyMetadata(0, OnTableSizeUpdated));

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(EncodingTableDisplay), new PropertyMetadata(0, OnTableSizeUpdated));

    public static readonly DependencyProperty CellTemplateProperty =
        DependencyProperty.Register(nameof(CellTemplate), typeof(DataTemplate), typeof(EncodingTableDisplay), new PropertyMetadata(null));

    public EncodingTableDisplay()
    {
        InitializeComponent();
    }

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public DataTemplate CellTemplate
    {
        get => (DataTemplate)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    public object[]? CellData
    {
        get => _cellData;
        set
        {
            _cellData = value;
            UpdateDisplay();
        }
    }

    private static void OnTableSizeUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EncodingTableDisplay etd)
            return;

        etd.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Initialize the table
        InitTable();
        InitCells();
    }

    private void InitTable()
    {
        // Clear existing definitions and children
        TableGrid.Children.Clear();
        TableGrid.ColumnDefinitions.Clear();
        TableGrid.RowDefinitions.Clear();

        // Add column definitions
        TableGrid.ColumnDefinitions.Add(new ColumnDefinition());
        TableGrid.RowDefinitions.Add(new RowDefinition());

        for (int i = 0; i < Columns; i++)
        {
            TableGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var label = new ContentControl
            {
                Content = new Label(i, Columns),
                ContentTemplate = LabelTemplate,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetColumn(label, i + 1);
            TableGrid.Children.Add(label);
        }

        for (int j = 0; j < Rows; j++)
        {
            TableGrid.RowDefinitions.Add(new RowDefinition());
            var label = new ContentControl
            {
                Content = new Label(j, Rows),
                ContentTemplate = LabelTemplate,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetRow(label, j + 1);
            TableGrid.Children.Add(label);
        }
    }

    private void InitCells()
    {
        if (CellData == null)
            return;

        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                int index = i + j * Columns;
                if (index >= CellData.Length)
                    return;

                var cell = new ContentControl
                {
                    Content = CellData[index],
                    ContentTemplate = CellTemplate,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                };
                Grid.SetColumn(cell, i + 1);
                Grid.SetRow(cell, j + 1);
                TableGrid.Children.Add(cell);
            }
        }
    }

    public static string GetLabel(int index, int count)
    {
        var bits = BitOperations.Log2((uint)count);
        return string.Format($"{{0:b{bits}}}", index);
    }
}

struct Label(int index, int count)
{
    public int Index { get; set; } = index;
    public int Count { get; set; } = count;
}
