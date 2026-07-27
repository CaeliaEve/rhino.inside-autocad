using Brush = System.Windows.Media.Brush;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <summary>
/// One AutoCAD Color Index and the brush which draws it, for display in a color picker.
/// </summary>
/// <seealso cref="AciColorPalette"/>
public class AciColorSwatch
{
    /// <summary>
    /// The AutoCAD Color Index this swatch stands for.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The color AutoCAD draws <see cref="Index"/> in, as a frozen brush.
    /// </summary>
    public Brush Brush { get; }

    /// <summary>
    /// The name of the color as it is shown to the user, for example "ACI 4".
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Constructs a new <see cref="AciColorSwatch"/>.
    /// </summary>
    /// <param name="index">The AutoCAD Color Index.</param>
    /// <param name="brush">The frozen brush which draws the index's color.</param>
    public AciColorSwatch(int index, Brush brush)
    {
        this.Index = index;
        this.Brush = brush;
        this.DisplayName = string.Format(UIConstants.AciColorNameFormat, index);
    }
}
