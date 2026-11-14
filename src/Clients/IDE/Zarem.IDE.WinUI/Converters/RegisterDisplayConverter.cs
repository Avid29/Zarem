// Avishai Dernis 2026

using Microsoft.UI.Xaml.Data;
using System;
using Zarem.Helpers;
using Zarem.IDE.Models.Enums;

namespace Zarem.IDE.Converters;

/// <summary>
/// An <see cref="IValueConverter"/> that handles displaying register values.
/// </summary>
public partial class RegisterDisplayConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="SymbolResolver"/> to use in conversion.
    /// </summary>
    public SymbolResolver? SymbolResolver { get; set; }

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not ulong val)
            return null;

        if (parameter is not RegisterDisplayMode mode)
            return null;

        return mode switch
        {
            RegisterDisplayMode.Label => GetLabelValue(val),

            // Simple cases
            RegisterDisplayMode.Binary => $"0b{val:B}",
            RegisterDisplayMode.Octal => $"0o{System.Convert.ToString((long)val, 8)}",
            RegisterDisplayMode.Hex => $"0x{val:X8}",
            RegisterDisplayMode.Decimal or _ => $"{val}",
        };
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private string GetLabelValue(ulong address)
    {
        if (SymbolResolver is null)
            return "N/A";

        var symbol = SymbolResolver.FindNearest(address, out _);
        if (symbol is null)
            return "N/A";

        var symOffset = address - symbol.Address.VirtualAddress;
        return $"{symbol.Name}+0x{symOffset:X4}";
    }
}
