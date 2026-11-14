// Avishai Dernis 2026

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using Zarem.DebugSessions;
using Zarem.Emulator.Extensions;
using Zarem.Helpers;
using Zarem.IDE.Models.Enums;

namespace Zarem.IDE.Controls;

public sealed partial class RegisterDisplay : UserControl
{
    public static readonly DependencyProperty RegisterNameProperty =
        DependencyProperty.Register(nameof(RegisterName), typeof(string), typeof(RegisterDisplay), new PropertyMetadata(string.Empty));
    
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(ulong), typeof(RegisterDisplay), new PropertyMetadata(0UL));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(RegisterDisplayMode), typeof(RegisterDisplay), new PropertyMetadata(RegisterDisplayMode.Decimal));

    public static readonly DependencyProperty SymbolResolverProperty =
        DependencyProperty.Register(nameof(SymbolResolver), typeof(SymbolResolver), typeof(RegisterDisplay), new PropertyMetadata(null));

    public static readonly DependencyProperty SessionProperty =
        DependencyProperty.Register(nameof(Session), typeof(DebugSession), typeof(RegisterDisplay), new PropertyMetadata(null));

    public RegisterDisplay()
    {
        this.InitializeComponent();
    }

    public string RegisterName
    {
        get => (string)GetValue(RegisterNameProperty);
        set => SetValue(RegisterNameProperty, value);
    }

    public ulong Value
    {
        get => (ulong)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public RegisterDisplayMode Mode
    {
        get => (RegisterDisplayMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public SymbolResolver? SymbolResolver
    {
        get => (SymbolResolver)GetValue(SymbolResolverProperty);
        set => SetValue(SymbolResolverProperty, value);
    }

    public DebugSession? Session
    {
        get => (DebugSession)GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    private string GetFormatedValue(RegisterDisplayMode mode, ulong value)
    {
        return mode switch
        {
            RegisterDisplayMode.Label => GetLabelValue(value),
            RegisterDisplayMode.String => GetStringValue(value),

            RegisterDisplayMode.Binary => $"0b{value:B}",
            RegisterDisplayMode.Octal => $"0o{Convert.ToString((long)value, 8)}",
            RegisterDisplayMode.Hex => $"0x{value:X8}",
            RegisterDisplayMode.Decimal or _ => $"{value}",
        };
    }

    private string GetLabelValue(ulong address)
    {
        if (SymbolResolver is null)
            return "N/A";

        var symbol = SymbolResolver.FindNearest(address, out _);
        if (symbol is null)
            return "N/A";

        var symOffset = address - symbol.Address.VirtualAddress;
        var lastIndex = symbol.Name.LastIndexOf(':') + 1;
        var symbolName = symbol.Name[lastIndex..];

        if (symOffset is 0)
        {
            return symbolName;
        }

        return $"{symbolName}+0x{symOffset:X}";
    }

    private string GetStringValue(ulong address)
    {
        if (Session is null)
            return "N/A";

        return $"\"{Session.Emulator.Computer.Memory.Virtual.ReadString(address, Encoding.ASCII, 256)}\"";
    }
}
