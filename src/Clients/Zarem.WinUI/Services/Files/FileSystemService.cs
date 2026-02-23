// Adam Dernis 2024

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Zarem.Services;
using Zarem.Services.Files;
using Zarem.Services.Files.Models;
using Zarem.Services.Popup;
using Zarem.Services.Popup.Enums;
using Zarem.Services.Popup.Models;
using Zarem.WinUI.Services.Files.Models;
using File = Zarem.WinUI.Services.Files.Models.File;
using Folder = Zarem.WinUI.Services.Files.Models.Folder;

namespace Zarem.WinUI.Services.Files;

/// <summary>
/// An <see cref="IFileSystemService"/> implementation wrapping <see cref="StorageFile"/>.
/// </summary>
public class FileSystemService : IFileSystemService
{
    private const string FolderNameRegex = "^[^\\s^\\x00-\\x1f\\\\?*:\"\";<>|\\/.][^\\x00-\\x1f\\\\?*:\"\";<>|\\/]*[^\\s^\\x00-\\x1f\\\\?*:\"\";<>|\\/.]+$";

    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemService"/> class.
    /// </summary>
    public FileSystemService(ILocalizationService localizationService, IPopupService popupService)
    {
        _localizationService = localizationService;
        _popupService = popupService;
    }

    /// <inheritdoc/>
    public async Task<IFile?> CreateFileAsync(string path)
    {
        // Split the path
        var folderPath = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);

        return await CreateFileAsync(folderPath, fileName);
    }

    /// <inheritdoc/>
    public async Task<IFile?> CreateFileByPopupAsync(string folder, string regex = "^[\\w\\-\\._\\(\\)\\[\\] ]+$")
    {
        var popup = new TextInputPopupDetails(_localizationService["/Popups/CreateNewFileTitle"])
        {
            PrimaryButtonText = _localizationService["/Popups/Create"],
            CloseButtonText = _localizationService["/Popups/Cancel"],
            ValidationRegex = regex,
        };

        var fileName = await _popupService.ShowPopupAsync(popup);
        if (fileName is null)
            return null;

        return await CreateFileAsync(folder, fileName);
    }

    private static async Task<IFile?> CreateFileAsync(string? folderPath, string fileName)
    {
        try
        {
            // Create the file in the parent folder.
            var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            var file = await folder.CreateFileAsync(fileName);
            return new File(file);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IFolder?> CreateFolderAsync(string path)
    {
        // Split the path
        var folderPath = Path.GetDirectoryName(path);
        var folderName = Path.GetFileName(path);

        return await CreateFolderAsync(folderPath, folderName);
    }

    /// <inheritdoc/>
    public async Task<IFolder?> CreateFolderByPopupAsync(string parent)
    {
        var popup = new TextInputPopupDetails(_localizationService["/Popups/CreateNewFolderTitle"])
        {
            PrimaryButtonText = _localizationService["/Popups/Create"],
            CloseButtonText = _localizationService["/Popups/Cancel"],
            ValidationRegex = FolderNameRegex,
        };

        var folderName = await _popupService.ShowPopupAsync(popup);
        if (folderName is null)
            return null;

        return await CreateFolderAsync(parent, folderName);
    }

    private static async Task<IFolder?> CreateFolderAsync(string? folderPath, string folderName)
    {
        try
        {
            // Create the file in the parent folder.
            var parent = await StorageFolder.GetFolderFromPathAsync(folderPath);
            var folder = await parent.CreateFolderAsync(folderName);
            return new Folder(folder);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IFile?> GetFileAsync(string path)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            return new File(file);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IFolder?> GetFolderAsync(string path)
    {
        try
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            return new Folder(folder);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFileItemAsync(IFileItem item, bool confirm = true)
    {
        if (confirm)
        {
            // Get the resource keys
            var (titleKey, descKey) = item switch
            {
                IFolder => ("/Popups/DeleteFolderTitle", "/Popups/DeleteFolderDescription"),
                IFile or _ => ("/Popups/DeleteFileTitle", "/Popups/DeleteFileDescription"),
            };

            // Load the resources
            var title = _localizationService[titleKey, item.Name];
            var desc = _localizationService[descKey, item.Name];

            var popup = new PopupDetails(title, desc)
            {
                PrimaryButtonText = _localizationService["/Popups/Confirm"],
                CloseButtonText = _localizationService["/Popups/Cancel"],
            };

            var confirmation = await _popupService.ShowPopupAsync(popup);
            if (confirmation is not PopupResult.Primary)
                return false;
        }

        await item.DeleteAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<IFile?> PickFileAsync(params string[] types)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List
        };

        if (types.Length is 0)
            types = ["*"];

        foreach (var type in types)
        {
            picker.FileTypeFilter.Add(type);
        }

        InitWindow(picker);
        StorageFile file = await picker.PickSingleFileAsync();

        if (file is null)
            return null;

        return new File(file);
    }

    /// <inheritdoc/>
    public async Task<IFolder?> PickFolderAsync()
    {
        var picker = new FolderPicker
        {
            ViewMode = PickerViewMode.List,
            FileTypeFilter = { "*" },
        };

        InitWindow(picker);
        var storageFolder = await picker.PickSingleFolderAsync();

        if (storageFolder is null)
            return null;

        return new Folder(storageFolder);
    }

    /// <inheritdoc/>
    public async Task<IFile?> PickSaveFileAsync(string filename)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.Unspecified,
            DefaultFileExtension = ".asm",
            SuggestedFileName = filename,
        };

        InitWindow(picker);
        var storageFile = await picker.PickSaveFileAsync();

        if (storageFile is null)
            return null;

        return new File(storageFile);
    }

    private void InitWindow(object picker)
    {
        nint windowHandle = WindowNative.GetWindowHandle(App.Current.Window);
        InitializeWithWindow.Initialize(picker, windowHandle);
    }
}
