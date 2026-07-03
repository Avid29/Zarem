// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Zarem.Debugger.Models;
using Zarem.Debugger.Viewer;
using Zarem.IDE.Models.Enums;

namespace Zarem.IDE.Bindables;

/// <summary>
/// A bindable wrapper for viewing a register.
/// </summary>
public class BindableRegister : ObservableObject
{
    private readonly IRegisterViewer _group;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindableRegister"/> class.
    /// </summary>
    public BindableRegister(RegisterMeta registerMeta, IRegisterViewer group)
    {
        _group = group;
        RegisterMeta = registerMeta;
    }

    /// <summary>
    /// Gets the register's name.
    /// </summary>
    public RegisterMeta RegisterMeta { get; }

    /// <summary>
    /// Get's the register's value
    /// </summary>
    public ulong Value
    {
        get => _group[RegisterMeta.Name] ?? 0;
        set
        {
            _group[RegisterMeta.Name] = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the register's display mode.
    /// </summary>
    public RegisterDisplayMode DisplayMode
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Invalidates the current <see cref="Value"/> and invokes a property changed.
    /// </summary>
    public void Invalidate()
    {
        OnPropertyChanged(nameof(Value));
    }
}
