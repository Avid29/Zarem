// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using System.Threading.Tasks;
using Zarem.IDE.Messages.Editor;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Messages.Files;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.IDE.ViewModels.Pages.Settings;

namespace Zarem.IDE.ViewModels;

public partial class WindowViewModel
{
    #region File

    [RelayCommand]
    private void OpenCreateProject() => MainViewModel.GoToPageByType<CreateProjectViewModel>();

    [RelayCommand]
    private async Task SaveFileAsync() => MainViewModel.FocusedPanel?.SaveFileAsync();

    [RelayCommand]
    private async Task SaveAllFilesAsync() => await MainViewModel.SaveAllFilesAsync();

    [RelayCommand]
    private async Task PickAndOpenFileAsync() => await MainViewModel.PickAndOpenFileAsync();

    [RelayCommand]
    private async Task PickAndOpenFolderAsync() => await MainViewModel.PickAndOpenFolderAsync();

    [RelayCommand]
    private async Task PickAndOpenProjectAsync() => await MainViewModel.PickAndOpenProjectAsync();

    [RelayCommand]
    private async Task ClosePageAsync()
    {
        var panel = MainViewModel.FocusedPanel;
        if (panel is null)
            return;

        await panel.ClosePageAsync(null);
    }

    [RelayCommand]
    private async Task CloseProjectAsync() => await _projectService.CloseProjectAsync();

    #endregion

    #region Edit

    [RelayCommand]
    private void SendEditOperation(EditorOperation operation) => _messenger.Send(new EditorOperationRequestMessage(operation));

    #endregion

    #region View

    [RelayCommand]
    private void ToggleFullScreenMode() => _windowingService.ToggleFullScreen();

    [RelayCommand]
    private void ShowConsole() => _consoleService.ShowConsoleWindow();

    #endregion

    #region Build

    [RelayCommand]
    private async Task BuildProjectAsync() => await _buildService.BuildProjectAsync();

    [RelayCommand]
    private async Task RebuildProjectAsync() => await _buildService.BuildProjectAsync(true);

    [RelayCommand]
    private async Task AssembleOpenFilesAsync()
    {
        var openSourceFiles = OpenFiles
            .Where(x => x.SourceFile is not null)
            .Select(x => x.SourceFile!);

        await _buildService.AssembleFilesAsync(openSourceFiles);
    }

    [RelayCommand]
    private async Task AssembleFileAsync()
    {
        // Check if the file is null
        var file = CurrentFile?.SourceFile;
        if (file is null)
            return;

        // Request to assemble the file
        await _buildService.AssembleFilesAsync([file]);
    }

    [RelayCommand]
    private void CleanProject() => _buildService.CleanProject();

    [RelayCommand]
    private void CleanOpenFiles()
    {
        var openSourceFiles = OpenFiles
            .Where(x => x.SourceFile is not null)
            .Select(x => x.SourceFile!);

        _buildService.CleanFiles(openSourceFiles);
    }

    [RelayCommand]
    private void CleanFile()
    {
        // Check if the file is null
        var file = CurrentFile?.SourceFile;
        if (file is null)
            return;

        // Request to assemble the file
        _buildService.CleanFiles([file]);
    }

    #endregion

    #region Debug

    [RelayCommand]
    private async Task StartDebuggingAsync() => await _debugService.RunAsync(true);

    [RelayCommand]
    private async Task StartWithoutDebuggingAsync() => await _debugService.RunAsync(false);

    [RelayCommand]
    private async Task StartFileWithoutDebuggingAsync()
    {
        // Gets file page view model if possible
        if (CurrentFile is null)
            return;

        // Gets the source file
        var sourceFile = CurrentFile.SourceFile;
        if (sourceFile is null)
            return;

        // Run the file
        await _debugService.RunFileAsync(sourceFile, false);
    }

    [RelayCommand]
    private async Task DebugResume() => _debugService.Resume();

    [RelayCommand]
    private async Task StopDebugging() => _debugService.StopDebugging();

    #endregion

    #region Help

    [RelayCommand]
    private void OpenAbout() => MainViewModel.GoToPageByType<AboutPageViewModel>();

    [RelayCommand]
    private void OpenMIPSCheatSheet() => MainViewModel.GoToPageByType<CheatSheetViewModel>();

    [RelayCommand]
    private void OpenWelcome() => MainViewModel.GoToPageByType<WelcomePageViewModel>();

    [RelayCommand]
    private void OpenSettings() => MainViewModel.GoToPageByType<SettingsPageViewModel>();

    #endregion
}
