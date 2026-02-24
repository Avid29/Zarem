// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zarem.Assembler.Logging;
using Zarem.Messages.Build;
using Zarem.Models;
using Zarem.Models.Enums;
using Zarem.Models.Files;
using Zarem.Services.Settings;
using Zarem.ViewModels;

namespace Zarem.Services;

/// <summary>
/// A service for managing the build status.
/// </summary>
public class BuildService : IBuildService
{
    private readonly IMessenger _messenger;
    private readonly ILocalizationService _localizationService;
    private readonly IProjectService _projectService;
    private readonly ISettingsService _settingsService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildService"/> class.
    /// </summary>
    public BuildService(
        IMessenger messenger,
        ILocalizationService localizationService,
        IProjectService projectService,
        ISettingsService settingsService,
        IStateService stateService)
    {
        _messenger = messenger;
        _localizationService = localizationService;
        _projectService = projectService;
        _settingsService = settingsService;
        _stateService = stateService;
    }

    /// <inheritdoc/>
    public async Task<BuildResult?> BuildProjectAsync(bool rebuild = false)
    {
        // TODO: Report issue
        if (_projectService.Project is null)
            return null;

        // TODO: Consider making a popup asking about saving
        await Service.Get<MainViewModel>().SaveAllFilesAsync();

        var logger = new Logger();
        var buildFunc = async () => await _projectService.Project.BuildProjectAsync(rebuild, logger);
        return await BuildAsync(buildFunc, logger);
    }

    /// <inheritdoc/>
    public async Task<BuildResult?> AssembleFilesAsync(IEnumerable<SourceFile> files)
    {
        // TODO: Report issue
        if (_projectService.Project is null)
            return null;

        // TODO: Consider making a popup asking about saving
        // TODO: Save only assembling files
        await Service.Get<MainViewModel>().SaveAllFilesAsync();

        var logger = new Logger();
        var buildFunc = async () => await _projectService.Project.AssembleFilesAsync(files, true, logger);
        return await BuildAsync(buildFunc, logger);
    }

    /// <inheritdoc/>
    public void CleanProject()
    {
        if (_projectService.Project is null)
            return;

        _projectService.Project.CleanProject();
    }

    /// <inheritdoc/>
    public void CleanFiles(IEnumerable<SourceFile> files)
    {
        if (_projectService.Project is null)
            return;

        _projectService.Project.CleanFiles(files);
    }

    private async Task<BuildResult?> BuildAsync(Func<Task<BuildResult?>> buildFunction, Logger logger)
    {
        // Run pre-build checks
        if (!_stateService.IsReady)
            return null;

        _stateService.SetState(IdeState.Building);

        // Culminate results
        _messenger.Send(new BuildStartedMessage(logger));

        // Override the current language
        var lang = _settingsService.Local.GetValue<string>(SettingsKeys.AssemblerLanguageOverride);
        var restore = CultureInfo.CurrentUICulture;
        if (lang is not null)
        {
            var newCulture = CultureInfo.GetCultureInfo(lang);
            CultureInfo.CurrentUICulture = newCulture ?? restore;
        }

        // Run the build task
        var result = await buildFunction();

        // Restore the original language
        CultureInfo.CurrentUICulture = restore;

        // Send a message with the build results.
        var message = ConstructMessage(result);
        var status = logger.Failed ? IdeState.BuildFailed : IdeState.BuildCompleted;
        _stateService.SetState(status, message);

        _messenger.Send(new BuildFinishedMessage(result));
        return result;
    }

    private string? ConstructMessage(BuildResult? result)
    {
        if (result is null)
            return null;

        StringBuilder message = new StringBuilder();

        void Append(string oneKey, string multiKey, int value)
        {
            if (value is 0)
                return;

            if (message.Length is not 0)
                message.Append(" - ");
            
            var key = value is 1 ? oneKey : multiKey;
            message.Append(_localizationService[key, value]);
        }

        Append("BuildStatus/OneSucceeded", "BuildStatus/Succeeded", result.SucessfullyAssembledFiles.Count);
        Append("BuildStatus/OneFailed", "BuildStatus/Failed", result.FailedFiles.Count);
        Append("BuildStatus/OneSkipped", "BuildStatus/Skipped", result.SkippedFiles.Count);
        return $"{message}";
    }
}
