namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a band from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// ProfileView bands display additional data (station labels, elevation labels, etc.)
/// at the top or bottom of a profile view.
/// </remarks>
public interface ICivilProfileViewBand
{
    /// <summary>
    /// Gets the name of the band.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the band type as a string.
    /// </summary>
    /// <value>
    /// Common values: "ProfileData", "VerticalGeometry", "HorizontalGeometry",
    /// "SuperElevation", "SectionData", etc.
    /// </value>
    string BandType { get; }

    /// <summary>
    /// Gets the name of the style applied to this band.
    /// </summary>
    string StyleName { get; }

    /// <summary>
    /// Gets the location of the band.
    /// </summary>
    /// <value>
    /// "Top" or "Bottom" indicating where the band is positioned.
    /// </value>
    string Location { get; }

    /// <summary>
    /// Gets a value indicating whether the band is visible.
    /// </summary>
    bool IsVisible { get; }
}
