// Adam Dernis 2024

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.IO;
using Zarem.Bindables.Files;
using Zarem.Bindables.Files.Abstract;
using Zarem.Bindables.Files.Interfaces;
using Zarem.Services;
using Zarem.ViewModels.Pages;
using Zarem.Windows.Controls;

namespace Zarem.Windows.Views.Pages;

/// <summary>
/// The explorer view.
/// </summary>
public sealed partial class Explorer : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Explorer"/> class.
    /// </summary>
    public Explorer()
    {
        this.InitializeComponent();
        DataContext = Service.Get<ExplorerViewModel>();
    }

    private ExplorerViewModel ViewModel => (ExplorerViewModel)DataContext;

    private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem tvi)
            return;

        var node = FilesTreeViewRoot.NodeFromContainer(tvi);
        if (node is null)
            return;

        if (node.Depth is 0)
        {
            FilesTreeViewRoot.Expand(node);
        }
    }

    private void FileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not TreeViewItem tvi || tvi.DataContext is not BindableFile file)
            return;

        file.Open();
    }

    private void FolderDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not TreeViewItem tvi)
            return;

        // Toggle expansion
        tvi.IsExpanded = !tvi.IsExpanded;
    }

    private async void TreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
    {
        if (args.Item is not IBindableFileItem item || !item.ChildrenNotLoaded)
            return;

        // Load children and ensure expansion
        await item.LoadChildrenAsync();
        args.Node.IsExpanded = true;
    }

    private async void RecentFileClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton btn)
            return;

        if (btn.Tag is not string path)
            return;

        await Service.Get<IProjectService>().OpenPathAsyc(path);
    }

    private static bool IsNull(object? obj) => obj is null;

    private static bool IsNotNull(object? obj) => obj is not null;

    private static string FormatPath(string path)
    {
        // TODO: Localization
        var localization = Service.Get<ILocalizationService>();

        if (Path.HasExtension(path))
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        else
        {
            var dirInfo = new DirectoryInfo(path);
            var parentName = dirInfo.Parent?.Name ?? string.Empty;
            var name = dirInfo.Name;
            return localization["/Pages/Explorer/RecentFolderListItem", parentName, name];
        }
    }

    private void RenameClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem mfi)
            return;

        var container = FilesTreeViewRoot.ContainerFromItem(mfi.DataContext);
        var editblock = container.FindDescendant<EditableTextBlock>();

        if (editblock is null)
            return;

        editblock.BeginEdit();
    }
}
