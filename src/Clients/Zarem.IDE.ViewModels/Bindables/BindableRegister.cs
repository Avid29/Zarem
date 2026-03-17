// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using Zarem.Debugger.Viewer;

namespace Zarem.IDE.Bindables;

/// <summary>
/// A bindable wrapper for viewing a register.
/// </summary>
public class BindableRegister : ObservableObject
{
    private readonly IRegisterGroup _group;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindableRegister"/> class.
    /// </summary>
    public BindableRegister(string registerName, IRegisterGroup group)
    {
        _group = group;
        RegisterName = registerName;
    }

    /// <summary>
    /// Gets the register's name.
    /// </summary>
    public string RegisterName { get; }

    /// <summary>
    /// Get's the register's value
    /// </summary>
    public ulong Value
    {
        get => _group[RegisterName] ?? 0;
        set
        {
            _group[RegisterName] = value;
            OnPropertyChanged(nameof(Value));
        }
    }
}
