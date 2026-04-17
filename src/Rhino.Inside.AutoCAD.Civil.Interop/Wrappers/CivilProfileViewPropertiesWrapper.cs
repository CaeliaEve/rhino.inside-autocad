using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted ProfileView property information.
/// The data is captured at construction time from a <see cref="ProfileView"/>.
/// </remarks>
public class CivilProfileViewPropertiesWrapper : ICivilProfileViewProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public double StationStart { get; }

    /// <inheritdoc />
    public double StationEnd { get; }

    /// <inheritdoc />
    public double ElevationMin { get; }

    /// <inheritdoc />
    public double ElevationMax { get; }

    /// <inheritdoc />
    public string AlignmentName { get; }

    /// <inheritdoc />
    public int ProfileCount { get; }

    /// <inheritdoc />
    public int BandCount { get; }

    /// <inheritdoc />
    public double HorizontalScale { get; }

    /// <inheritdoc />
    public double VerticalScale { get; }

    /// <inheritdoc />
    public double VerticalExaggeration { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileViewPropertiesWrapper"/>
    /// from a Civil 3D ProfileView.
    /// </summary>
    /// <param name="profileView">The ProfileView to extract properties from.</param>
    /// <param name="alignmentName">The name of the parent alignment.</param>
    /// <param name="profileCount">The number of profiles displayed in this view.</param>
    public CivilProfileViewPropertiesWrapper(ProfileView profileView, string alignmentName, int profileCount)
    {
        this.Name = profileView.Name;
        this.Description = profileView.Description ?? string.Empty;
        this.StationStart = profileView.StationStart;
        this.StationEnd = profileView.StationEnd;
        this.ElevationMin = profileView.ElevationMin;
        this.ElevationMax = profileView.ElevationMax;
        this.AlignmentName = alignmentName;
        this.ProfileCount = profileCount;

        // Count bands from both top and bottom
        var topBandCount = profileView.Bands.GetTopBandItems()?.Count ?? 0;
        var bottomBandCount = profileView.Bands.GetBottomBandItems()?.Count ?? 0;
        this.BandCount = topBandCount + bottomBandCount;

        // Scale information - using default values
        // TODO: Determine correct API to extract scale from ProfileView
        this.HorizontalScale = 1.0;
        this.VerticalScale = 1.0;
        this.VerticalExaggeration = 1.0;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileViewPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilProfileViewPropertiesWrapper(
        string name,
        string description,
        double stationStart,
        double stationEnd,
        double elevationMin,
        double elevationMax,
        string alignmentName,
        int profileCount,
        int bandCount,
        double horizontalScale,
        double verticalScale,
        double verticalExaggeration)
    {
        this.Name = name;
        this.Description = description;
        this.StationStart = stationStart;
        this.StationEnd = stationEnd;
        this.ElevationMin = elevationMin;
        this.ElevationMax = elevationMax;
        this.AlignmentName = alignmentName;
        this.ProfileCount = profileCount;
        this.BandCount = bandCount;
        this.HorizontalScale = horizontalScale;
        this.VerticalScale = verticalScale;
        this.VerticalExaggeration = verticalExaggeration;
    }

    /// <summary>
    /// Creates a duplicate of this ProfileView properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewPropertiesWrapper Duplicate()
    {
        return new CivilProfileViewPropertiesWrapper(
            this.Name,
            this.Description,
            this.StationStart,
            this.StationEnd,
            this.ElevationMin,
            this.ElevationMax,
            this.AlignmentName,
            this.ProfileCount,
            this.BandCount,
            this.HorizontalScale,
            this.VerticalScale,
            this.VerticalExaggeration);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Properties: {this.Name} (Sta: {this.StationStart:F2} - {this.StationEnd:F2}, Elev: {this.ElevationMin:F2} - {this.ElevationMax:F2})";
    }
}
