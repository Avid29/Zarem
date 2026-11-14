// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Zarem.Emulator.Models.Enums;

namespace Zarem.SAE.ViewModels;

public partial class WindowViewModel
{
    #region File

    [RelayCommand]
    private async Task ConfigureBIOS()
    {
        // TODO: Move to emulation service or something more centralized.
        var biosRom = await _fileSystemService.PickFileAsync([".bin", ".rom"]);
        if (biosRom is null)
            return;

        _settingsService.Local.SetValue(SettingsKeys.BiosRomPath, biosRom?.Path);
    }

    #endregion

    #region Execution

    [RelayCommand]
    private async Task TogglePower()
    {
        switch (_emulatorService.State)
        {
            case EmulatorState.Ready:
            case EmulatorState.Stopped:
                _emulatorService.StartEmulator();
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

    //[RelayCommand]
    //private void OpenAbout() => MainViewModel.GoToPageByType<AboutPageViewModel>();

    //[RelayCommand]
    //private void OpenSettings() => MainViewModel.GoToPageByType<SettingsPageViewModel>();

    #endregion
}
