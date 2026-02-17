// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using Zarem.Assembler.Config;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Bindables.Files;
using Zarem.Messages;
using Zarem.Messages.Editor.Enums;
using Zarem.MIPS;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.Services.Settings.Enums;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels.Pages;

/// <summary>
/// A view model for a file page.
/// </summary>
public partial class FilePageViewModel : PageViewModel
{
    private readonly IMessenger _messenger;
    private readonly ISettingsService _settingsService;
    private readonly IProjectService _projectService;

    /// <summary>
    /// An event invoked requesting to navigate to a token.
    /// </summary>
    public event EventHandler<SourceLocation>? NavigateToTokenEvent;

    /// <summary>
    /// An event invoked when an editor operation is requested.
    /// </summary>
    public event EventHandler<EditorOperation>? EditorOperationRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePageViewModel"/> class.
    /// </summary>
    public FilePageViewModel(IMessenger messenger, ISettingsService settingsService, IProjectService projectService)
    {
        _messenger = messenger;
        _settingsService = settingsService;
        _projectService = projectService;

        IsActive = true;
    }

    /// <summary>
    /// Gets or sets whether or not the file should be viewed with the generic text editor, regardless of type.
    /// </summary>
    public bool ForceTextEditor { get; set; }

    /// <summary>
    /// Gets the bindable file for this page.
    /// </summary>
    public BindableFile? File
    {
        get;
        set
        {
            var old = field;
            if (SetProperty(ref field, value))
            {
                if (old is not null)
                {
                    old.PropertyChanged -= OnFileUpdate;
                }

                if (field is not null)
                {
                    field.PropertyChanged += OnFileUpdate;
                }

                _ = LoadContentAsync();
            }
        }
    }

    /// <summary>
    /// Gets whether or not the file should be assembled in real-time.
    /// </summary>
    public bool AssembleRealTime => _settingsService.Local.GetValue<bool>(SettingsKeys.RealTimeAssembly);

    /// <summary>
    /// Gets the threshold for showing logs as annotations.
    /// </summary>
    public AnnotationThreshold AnnotationThreshold => _settingsService.Local.GetValue<AnnotationThreshold>(SettingsKeys.AnnotationThreshold);

    /// <summary>
    /// Gets the config to use for assembler checking.
    /// </summary>
    public MIPSAssemblerConfig? AssemblerConfig
    {
        get
        {
            if (_projectService.Project?.Config?.ArchitectureConfig is MIPSArchitectureConfig mips)
                return mips.AssemblerConfig;

            return null;
        }
    }

    /// <inheritdoc/>
    public override string Title => File?.Name ?? string.Empty;

    /// <inheritdoc/>
    public override bool CanTextEdit => true;

    /// <inheritdoc/>
    public override bool CanSave => true;

    /// <inheritdoc/>
    public override bool IsDirty => Content != OriginalContent;

    /// <inheritdoc/>
    public override bool CanAssemble => File?.SourceFile is not null;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<FilePageViewModel, SettingChangedMessage<AnnotationThreshold>>(this, (r, m) => OnPropertyChanged(nameof(AnnotationThreshold)));
        _messenger.Register<FilePageViewModel, SettingChangedMessage<bool>>(this, (r, m) =>
        {
            if (m.Key != SettingsKeys.RealTimeAssembly)
                return;

            OnPropertyChanged(nameof(AssembleRealTime));
        });
    }

    /// <summary>
    /// Requests to navigate to a token.
    /// </summary>
    /// <param name="token">The token to navigate to.</param>
    public void NavigateToToken(Token token) => NavigateToTokenEvent?.Invoke(this, token.Location);

    /// <summary>
    /// Requests an editor operation.
    /// </summary>
    /// <param name="operation">The editor operation requested.</param>
    public void ApplyOperation(EditorOperation operation) => EditorOperationRequested?.Invoke(this, operation);

    private void OnFileUpdate(object? sender, PropertyChangedEventArgs args)
    {
        OnPropertyChanged(nameof(Title));
    }
}
