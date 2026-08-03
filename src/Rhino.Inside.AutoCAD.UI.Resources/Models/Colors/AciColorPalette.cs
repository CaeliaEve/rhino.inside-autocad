using Rhino.Inside.AutoCAD.Services;
using System.Diagnostics;
using AutocadColor = Autodesk.AutoCAD.Colors.Color;
using AutocadColorMethod = Autodesk.AutoCAD.Colors.ColorMethod;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <summary>
/// The AutoCAD Color Index palette, as brushes a color picker can draw.
/// </summary>
/// <remarks>
/// The colors are looked up from AutoCAD rather than tabulated here, so a swatch always shows
/// what the preview will actually be drawn in. The brushes are frozen because the support
/// dialog runs on its own thread.
/// </remarks>
/// <seealso cref="AciColorSwatch"/>
public static class AciColorPalette
{
    private const int _minIndex = ApplicationConstants.MinAciColorIndex;
    private const int _maxIndex = ApplicationConstants.MaxAciColorIndex;

    private static readonly Lazy<IReadOnlyList<AciColorSwatch>> _swatches = new(Build);

    private static readonly Brush _fallbackBrush = CreateFrozenBrush(Colors.Gray);

    /// <summary>
    /// The swatch for every AutoCAD Color Index which names a color, in index order.
    /// </summary>
    public static IReadOnlyList<AciColorSwatch> Swatches => _swatches.Value;

    /// <summary>
    /// The brush shown in place of a color which is not in the palette.
    /// </summary>
    public static Brush FallbackBrush => _fallbackBrush;

    /// <summary>
    /// Returns the brush which draws the given AutoCAD Color Index, or grey when the index is
    /// outside the range of the palette.
    /// </summary>
    public static Brush GetBrush(int colorIndex)
    {
        if (colorIndex < _minIndex || colorIndex > _maxIndex)
            return _fallbackBrush;

        return Swatches[colorIndex - _minIndex].Brush;
    }

    /// <summary>
    /// Builds the swatch for every color index in the palette.
    /// </summary>
    private static IReadOnlyList<AciColorSwatch> Build()
    {
        var swatches = new List<AciColorSwatch>(_maxIndex - _minIndex + 1);

        for (var index = _minIndex; index <= _maxIndex; index++)
        {
            swatches.Add(new AciColorSwatch(index, CreateBrush(index)));
        }

        return swatches;
    }

    /// <summary>
    /// Returns the brush AutoCAD draws the given color index in, falling back to grey rather
    /// than throwing: a palette lookup must never be the reason the dialog fails to open.
    /// </summary>
    private static Brush CreateBrush(int colorIndex)
    {
        try
        {
            var autocadColor =
                AutocadColor.FromColorIndex(AutocadColorMethod.ByAci, (short)colorIndex);

            var rgb = autocadColor.ColorValue;

            return CreateFrozenBrush(Color.FromRgb(rgb.R, rgb.G, rgb.B));
        }
        catch (Exception e)
        {
            // Deliberately not logged: this runs while the palette is being built for the
            // dialog, where reaching for the logger would risk turning a missing color into
            // a dialog which does not open.
            Debug.WriteLine($"Could not look up ACI {colorIndex}: {e.Message}");

            return _fallbackBrush;
        }
    }

    /// <summary>
    /// Returns a frozen brush of the given color, safe to use from any thread.
    /// </summary>
    private static Brush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);

        brush.Freeze();

        return brush;
    }
}
