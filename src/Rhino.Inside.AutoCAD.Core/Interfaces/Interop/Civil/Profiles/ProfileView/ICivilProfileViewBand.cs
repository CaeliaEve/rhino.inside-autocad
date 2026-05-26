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
    /// Gets the band type.
    /// </summary>
    /// <value>
    /// Common values: "ProfileData", "VerticalGeometry", "HorizontalGeometry",
    /// "SuperElevation", "SectionData", etc.
    /// </value>
    CivilBandType BandType { get; }

    /// <summary>
    /// Gets the Id of the style applied to this band.
    /// </summary>
    IObjectId StyleId { get; }

    /// <summary>
    /// Gets the Id of the datasource of this band.
    /// </summary>
    IObjectId DataSourceId { get; }

    /// <summary>
    /// Gets the location of the band.
    /// </summary>
    /// <value>
    /// "Top" or "Bottom" indicating where the band is positioned.
    /// </value>
    CivilBandLocationType Location { get; }

    /// <summary>
    /// Gets the index of the band within the ProfileView's band collection.
    /// </summary>
    int Index { get; }
}
