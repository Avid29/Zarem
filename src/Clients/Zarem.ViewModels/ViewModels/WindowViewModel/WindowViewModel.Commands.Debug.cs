// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Zarem.ViewModels.Pages;

namespace Zarem.ViewModels;

public partial class WindowViewModel
{
    [RelayCommand]
    private async Task StartWithoutDebuggingAsync()
    {
        await _debugService.RunAsync(false);
    }

    [RelayCommand]
    private async Task StartFileWithoutDebuggingAsync()
    {
        // Gets file page view model if possible
        if (MainViewModel.FocusedPanel?.CurrentPage is not FilePageViewModel filePageViewModel)
            return;

        // Gets the source file
        var sourceFile = filePageViewModel.File?.SourceFile;
        if (sourceFile is null)
            return;

        // Run the file
        await _debugService.RunFileAsync(sourceFile, false);
    }
}
