// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using System.Threading.Tasks;
using Zarem.Assembler.Tokenization.Models;
using Zarem.IDE.Bindables.Files.Interfaces;
using Zarem.IDE.Messages.Editor;
using Zarem.IDE.Messages.Editor.Enums;
using Zarem.IDE.Messages.Files;
using Zarem.IDE.Messages.Navigation;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.ViewModels;

public partial class MainViewModel
{
    /// <inheritdoc/>
    protected override void OnActivated()
    {
        RegisterFileMessages();
        RegisterEditMessages();
        RegisterNavigationMessages();

        //_messenger.Register<MainViewModel, ProjectOpenedMessage>(this, (r, m) )
    }

    private void RegisterFileMessages()
    {
        _messenger.Register<MainViewModel, FileCreateNewRequestMessage>(this, (r, m) => r.CreateNewFile());
        _messenger.Register<MainViewModel, FileOpenRequestMessage>(this, (r, m) => r.OpenFile(m.File));
        _messenger.Register<MainViewModel, PageCloseRequestMessage>(this, async (r, m) =>
        {
            var panel = r.FocusedPanel;
            if (panel is null)
                return;

            await panel.ClosePageAsync(m.Page);
        });
    }

    private void RegisterEditMessages()
    {
        _messenger.Register<MainViewModel, EditorOperationRequestMessage>(this, (r, m) => r.ApplyEditorOperation(m.Operation));
    }

    private void RegisterNavigationMessages()
    {
        _messenger.Register<MainViewModel, PanelFocusChangedMessage>(this, (r, m) => r.FocusedPanel = m.FocusedPanel);
        _messenger.Register<MainViewModel, NavigateToTokenRequestMessage>(this, (r, m) => _ = r.NavigateToToken(m.Target));
    }

    /// <summary>
    /// Creates and opens a new anonymous file.
    /// </summary>
    public void CreateNewFile() => OpenFile(null);

    /// <summary>
    /// Picks and opens a file.
    /// </summary>
    public async Task PickAndOpenFileAsync()
    {
        // Select the file to open
        var file = await _fileService.PickFileAsync();
        if (file is null)
            return;

        OpenFile(file);
    }

    /// <summary>
    /// Picks and opens a folder.
    /// </summary>
    public async Task PickAndOpenFolderAsync()
    {
        // Select the folder to open
        var folder = await _fileSystemService.PickFolderAsync();
        if (folder is null)
            return;

        _projectService.OpenFolder(folder);
    }

    /// <summary>
    /// Picks and opens a folder.
    /// </summary>
    public async Task PickAndOpenProjectAsync()
    {
        // Select the project to open
        var project = await _fileSystemService.PickFileAsync(".zrmp");
        if (project?.Path is null)
            return;

        await _projectService.OpenProjectAsync(project.Path);
    }

    private FilePageViewModel OpenFile(IBindableFile? file, bool reopen = false)
    {
        // Create new file if needed
        if (file is null)
        {
            // TODO: Create file with generic name
        }

        // Check for existing page
        FilePageViewModel? page = null;
        if (!reopen)
        {
            page = FocusedPanel?.OpenPages.
                OfType<FilePageViewModel>()
                .FirstOrDefault(x => x.File == file);
        }

        // Create page view model if needed
        if (page is null)
        {
            page = Service.Get<FilePageViewModel>();
            page.File = file;
        }

        // Open the page
        FocusedPanel?.OpenPage(page);
        return page;
    }

    private async Task NavigateToToken(Token token)
    {
        // Get the file path
        var path = token.FilePath;
        if (path is null)
            return;

        // Get the file
        var file = await _fileService.GetFileAsync(path);
        if (file is null)
            return;

        // Open the file
        var page = OpenFile(file);

        // Navigate to the token
        page.NavigateToToken(token);
    }

    private void ApplyEditorOperation(EditorOperation operation)
    {
        if (FocusedPanel?.CurrentPage is not FilePageViewModel filePage)
            return;

        filePage.ApplyOperation(operation);
    }
}
