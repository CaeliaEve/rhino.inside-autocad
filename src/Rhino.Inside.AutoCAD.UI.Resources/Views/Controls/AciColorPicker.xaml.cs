using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace Rhino.Inside.AutoCAD.UI.Resources.Views;

/// <summary>
/// Picks an AutoCAD Color Index from the full palette, shown as a button carrying the current
/// color which opens a grid of swatches.
/// </summary>
public partial class AciColorPicker : UserControl
{
    /// <summary>
    /// Backs <see cref="SelectedColorIndex"/>.
    /// </summary>
    public static readonly DependencyProperty SelectedColorIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedColorIndex),
            typeof(int),
            typeof(AciColorPicker),
            new FrameworkPropertyMetadata(ApplicationConstants.MinAciColorIndex,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// The AutoCAD Color Index which is currently picked. Bound two-way by default, as the
    /// point of the control is to report the user's choice back.
    /// </summary>
    public int SelectedColorIndex
    {
        get => (int)this.GetValue(SelectedColorIndexProperty);
        set => this.SetValue(SelectedColorIndexProperty, value);
    }

    /// <summary>
    /// Constructs a new <see cref="AciColorPicker"/>.
    /// </summary>
    public AciColorPicker()
    {
        this.InitializeComponent();

        _swatches.ItemsSource = AciColorPalette.Swatches;
    }

    /// <summary>
    /// Opens the palette. It closes itself once the pointer goes elsewhere.
    /// </summary>
    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        _popup.IsOpen = true;
    }

    /// <summary>
    /// Picks the clicked swatch and closes the palette.
    /// </summary>
    private void Swatch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: AciColorSwatch swatch })
        {
            this.SelectedColorIndex = swatch.Index;
        }

        _popup.IsOpen = false;
    }
}
