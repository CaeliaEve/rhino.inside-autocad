using Rhino.Inside.AutoCAD.UI.Resources.Models;
using System.Globalization;
using System.Windows.Data;

namespace Rhino.Inside.AutoCAD.UI.Resources.Views;

/// <summary>
/// Converts an AutoCAD Color Index into the brush which draws it.
/// </summary>
/// <seealso cref="AciColorPalette"/>
public class AciColorBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return value is int colorIndex
            ? AciColorPalette.GetBrush(colorIndex)
            : AciColorPalette.FallbackBrush;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Not supported: a brush cannot be turned back into a color index, and the binding which
    /// uses this converter is one way.
    /// </remarks>
    public object? ConvertBack(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
