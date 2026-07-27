using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.UI.Resources.ViewModels;
using System.Windows.Input;

namespace Rhino.Inside.AutoCAD.UI.Resources.Views;

/// <summary>
/// Interaction logic for RhinoVersionDialogWindow.xaml
/// </summary>
public partial class RhinoVersionDialogWindow : IWindow
{
    private readonly RhinoVersionDialogViewModel _viewModel;

    /// <summary>
    /// Constructs a new <see cref="RhinoVersionDialogWindow"/>.
    /// </summary>
    /// <param name="viewModel">The view model backing the window.</param>
    public RhinoVersionDialogWindow(RhinoVersionDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        this.DataContext = viewModel;

        _viewModel.ChoiceMade += this.OnChoiceMade;
    }

    /// <summary>
    /// Closes the window once the user has made a choice.
    /// </summary>
    private void OnChoiceMade(object? sender, EventArgs e)
    {
        _viewModel.ChoiceMade -= this.OnChoiceMade;

        this.Close();
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
