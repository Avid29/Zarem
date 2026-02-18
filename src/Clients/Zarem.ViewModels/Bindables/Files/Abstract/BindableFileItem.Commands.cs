// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Input;
using Zarem.Services;
using System.Threading.Tasks;

namespace Zarem.Bindables.Files;

public partial class BindableFileItem
{
    /// <summary>
    /// Copies the file name to the clipboard.
    /// </summary>
    /// 
    [RelayCommand]
    public void CopyFileName() => Service.Get<IClipboardService>().CopyText(Name);

    /// <summary>
    /// Copies the file's path to the clipboard.
    /// </summary>
    [RelayCommand]
    public void CopyFilePath() => Service.Get<IClipboardService>().CopyText(Path);

    /// <summary>
    /// Copies the file to the clipboard.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public abstract Task CopyFileAsync();

    /// <summary>
    /// Deletes the file item.
    /// </summary>
    [RelayCommand]
    public abstract Task DeleteAsync();
}
