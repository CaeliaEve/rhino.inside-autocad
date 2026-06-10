using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.UI.Resources.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Rhino.Inside.AutoCAD.UI.Resources.Views;

/// <summary>
/// Interaction logic for ExpirationDialogWindow.xaml
/// </summary>
public partial class ExpirationDialogWindow : IWindow
{
    private readonly ExpirationDialogViewModel _viewModel;

    /// <summary>
    /// Constructs a new <see cref="ExpirationDialogWindow"/>.
    /// </summary>
    public ExpirationDialogWindow(ExpirationDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        this.DataContext = viewModel;
    }

    /// <summary>
    /// Closes the expiration dialog window.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// Opens the download URL in the default browser.
    /// </summary>
    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _viewModel.DownloadUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }

    /// <summary>
    /// Allows the window to be dragged by holding down the left mouse button.
    /// </summary>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }
}
