// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Zarem.Emulator.Models.Enums;
using Zarem.SAE.ViewModels.Pages;
using Zarem.SAE.ViewModels.Pages.Settings;

namespace Zarem.SAE.ViewModels;

public partial class WindowViewModel
{
    #region File

    [RelayCommand]
    private async Task InsertCartridgeAsync() => await _emulatorService.InsertCartridgeAsync();

    [RelayCommand]
    private void EjectCartridge() => _emulatorService.EjectCartridge();

    [RelayCommand]
    private async Task ConfigureBIOSAsync() => await _emulatorService.ConfigureBiosAsync();

    #endregion

    #region Execution

    [RelayCommand]
    private async Task TogglePowerAsync()
    {
        switch (_emulatorService.State)
        {
            case EmulatorState.Ready:
            case EmulatorState.Stopped:
                await _emulatorService.StartEmulatorAsync();
                break;
            case EmulatorState.Running:
            case EmulatorState.Pausing:
            case EmulatorState.Paused:
                _emulatorService.StopEmulator();
                break;
        }
    }

    #endregion

    #region View

    [RelayCommand]
    private void ToggleFullScreenMode() => _windowingService.ToggleFullScreen();

    #endregion

    #region Help

    [RelayCommand]
    private void OpenAbout() => OverlayViewModel.OpenPage<SaeAboutPageViewModel>();

    [RelayCommand]
    private void OpenSettings() => OverlayViewModel.OpenPage<SaeSettingsPageViewModel>();

    #endregion
}
