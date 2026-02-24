// Avishai Dernis 2024

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Text;
using Zarem.Messages.Navigation;
using Zarem.Services;
using Zarem.ViewModels;
using Zarem.ViewModels.Pages.Abstract;
using Zarem.WinUI.Helpers;
using Zarem.WinUI.Windows;

namespace Zarem.WinUI.Views.Shell;

/// <summary>
/// The main content tab view.
/// </summary>
public sealed partial class PanelView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PanelView"/> class.
    /// </summary>
    public PanelView()
    {
        this.InitializeComponent();

        this.DataContext = Service.Get<PanelViewModel>();
    }

    private PanelViewModel ViewModel => (PanelViewModel)DataContext;

    private async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not PageViewModel page)
            return;

        await ViewModel.ClosePageAsync(page);
    }

    private void UserControl_GotFocus(object sender, RoutedEventArgs e)
    {
        Service.Get<IMessenger>().Send(new PanelFocusChangedMessage(ViewModel));
    }

    private async void TabView_TabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
    {
        if (args.Item is not PageViewModel page)
            return;

        // Get current window and create new window
        var currentWindow = WindowHelper.GetWindowForElement(sender);
        var newWindow = WindowHelper.CreateWindow<PanelWindow>();
        newWindow.Activate();

        // Close the page on the current window and open on the new window
        currentWindow?.ViewModel.PanelViewModel.ClosePage(page);
        newWindow.ViewModel.PanelViewModel.OpenPage(page);
    }

    private static FontStyle UseItalics(bool state) => state ? FontStyle.Italic : FontStyle.Normal;
}
