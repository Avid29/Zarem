// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Zarem.Debugger.Viewer;
using Zarem.IDE.Models.Enums;
using Zarem.IDE.Services;

namespace Zarem.IDE.Bindables;

/// <summary>
/// A bindable wrapper for viewing a register.
/// </summary>
public class BindableRegister : ObservableObject, IDisposable
{
    private readonly IRegisterGroup _group;

    /// <summary>
    /// Initializes a new instance of the <see cref="BindableRegister"/> class.
    /// </summary>
    public BindableRegister(string registerName, IRegisterGroup group)
    {
        _group = group;
        RegisterName = registerName;

        _group.RegisterUpdated += OnRegisterUpdated;
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

    /// <summary>
    /// Gets or sets the register's display mode.
    /// </summary>
    public RegisterDisplayMode DisplayMode
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _group.RegisterUpdated -= OnRegisterUpdated;
    }

    private void OnRegisterUpdated(IRegisterGroup sender, string e)
    {
        if (e != RegisterName)
            return;

        Service.Get<IDispatcherService>().RunOnUIThread(() =>
        {
            OnPropertyChanged(nameof(Value));
        });
    }
}
