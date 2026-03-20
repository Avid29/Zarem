// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Zarem.Assembler.Config;
using Zarem.Assembler.Tokenization.Models;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Messages;
using Zarem.IDE.Messages.DebugSessions;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Settings;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.IDE.ViewModels.Pages.Abstract;
using Zarem.IDE.ViewModels.Pages.Interfaces;
using Zarem.MIPS;
using Zarem.Models.Tables;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for a file page.
/// </summary>
public partial class FilePageViewModel : PageViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDebugService _debugService;
    private readonly IProjectService _projectService;
    private readonly ISettingsService _settingsService;

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
    public FilePageViewModel(IMessenger messenger, IDebugService debugService, IProjectService projectService, ISettingsService settingsService)
    {
        _messenger = messenger;
        _debugService = debugService;
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
    public IBindableFile? File
    {
        get;
        set
        {
            var old = field;
            if (SetProperty(ref field, value))
            {
                old?.PropertyChanged -= OnFileUpdate;
                field?.PropertyChanged += OnFileUpdate;
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently executing line.
    /// </summary>
    public SourceRange? ExecutingLocation
    {
        get => _debugService.ExecutingLocation;
        set
        {
            _debugService.ExecutingLocation = value;
            OnPropertyChanged(nameof(ExecutingLocation));
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="IFileEditorHandler"/> for driving UI events.
    /// </summary>
    public IFileEditorHandler? EditorHandler { get; set; }

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
    public MipsAssemblerConfig? AssemblerConfig
    {
        get
        {
            if (_projectService.Project?.Config?.ArchitectureConfig is MipsArchitectureConfig mips)
                return mips.AssemblerConfig;

            return null;
        }
    }

    /// <inheritdoc/>
    public override string Title => File?.Name ?? string.Empty;

    /// <inheritdoc/>
    public override bool CanTextEdit => true;

    /// <inheritdoc/>
    public override bool CanSave => IsDirty;

    /// <inheritdoc/>
    public override bool IsDirty => EditorHandler?.IsDirty ?? false;

    /// <inheritdoc/>
    public override bool CanAssemble => File?.SourceFile is not null;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _messenger.Register<FilePageViewModel, ExecutingLocationChangedMessage>(this, (r, m) => r.OnPropertyChanged(nameof(ExecutingLocation)));
        _messenger.Register<FilePageViewModel, SettingChangedMessage<AnnotationThreshold>>(this, (r, m) => OnPropertyChanged(nameof(AnnotationThreshold)));
        _messenger.Register<FilePageViewModel, SettingChangedMessage<bool>>(this, (r, m) =>
        {
            if (m.Key != SettingsKeys.RealTimeAssembly)
                return;

            OnPropertyChanged(nameof(AssembleRealTime));
        });
    }

    /// <summary>
    /// Invokes the property changed event for relevant properties
    /// </summary>
    public void NotifyStateChanged()
    {
        // Notify the UI that IsDirty and CanSave might have changed
        OnPropertyChanged(nameof(IsDirty));
        OnPropertyChanged(nameof(CanSave));
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

    /// <inheritdoc/>
    public override async Task SaveAsync()
    {
        if (!IsDirty || EditorHandler is null)
            return;

        await EditorHandler.SaveAsync();
    }

    private void OnFileUpdate(object? sender, PropertyChangedEventArgs args)
    {
        OnPropertyChanged(nameof(Title));
    }
}
