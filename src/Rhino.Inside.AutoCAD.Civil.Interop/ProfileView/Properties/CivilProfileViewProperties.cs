using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted ProfileView property information.
/// The data is captured at construction time from a <see cref="ProfileView"/>.
/// </remarks>
public record CivilProfileViewProperties : ICivilProfileViewProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilProfileViewProperties"/> by extracting
    /// data from a given <see cref="ProfileView"/>.
    /// </summary>
    /// <param name="profileView">The ProfileView to extract properties from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    public static CivilProfileViewProperties CreateFromProfileView(
        ProfileView profileView,
        IAutocadTransactionManager transactionManager)
    {
        // Count bands from both top and bottom
        var topBandCount = profileView.Bands.GetTopBandItems()?.Count ?? 0;
        var bottomBandCount = profileView.Bands.GetBottomBandItems()?.Count ?? 0;

        // Get alignment name and profile count from the profile view
        var alignmentName = GetAlignmentName(profileView, transactionManager);
        var profileCount = GetProfileCount(profileView, transactionManager);

        return new CivilProfileViewProperties()
        {
            Name = profileView.Name,
            Description = profileView.Description ?? string.Empty,
            StationStart = profileView.StationStart,
            StationEnd = profileView.StationEnd,
            ElevationMin = profileView.ElevationMin,
            ElevationMax = profileView.ElevationMax,
            AlignmentName = alignmentName,
            ProfileCount = profileCount,
            BandCount = topBandCount + bottomBandCount,
            // Scale information - using default values
            // TODO: Determine correct API to extract scale from ProfileView
            HorizontalScale = 1.0,
            VerticalScale = 1.0,
            VerticalExaggeration = 1.0,
        };
    }

    /// <summary>
    /// Gets the parent alignment name for a ProfileView.
    /// </summary>
    private static string GetAlignmentName(ProfileView profileView, IAutocadTransactionManager transactionManager)
    {
        try
        {
            var alignmentId = profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return string.Empty;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            return alignment?.Name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the count of profiles displayed in the ProfileView.
    /// </summary>
    private static int GetProfileCount(ProfileView profileView, IAutocadTransactionManager transactionManager)
    {
        try
        {
            var alignmentId = profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return 0;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            return alignment?.GetProfileIds().Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public double StationStart { get; init; }

    /// <inheritdoc />
    public double StationEnd { get; init; }

    /// <inheritdoc />
    public double ElevationMin { get; init; }

    /// <inheritdoc />
    public double ElevationMax { get; init; }

    /// <inheritdoc />
    public string AlignmentName { get; init; } = string.Empty;

    /// <inheritdoc />
    public int ProfileCount { get; init; }

    /// <inheritdoc />
    public int BandCount { get; init; }

    /// <inheritdoc />
    public double HorizontalScale { get; init; }

    /// <inheritdoc />
    public double VerticalScale { get; init; }

    /// <inheritdoc />
    public double VerticalExaggeration { get; init; }

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilProfileViewProperties"/>
    /// </summary>
    private CivilProfileViewProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this ProfileView properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewProperties Duplicate()
    {
        return new CivilProfileViewProperties()
        {
            Name = this.Name,
            Description = this.Description,
            StationStart = this.StationStart,
            StationEnd = this.StationEnd,
            ElevationMin = this.ElevationMin,
            ElevationMax = this.ElevationMax,
            AlignmentName = this.AlignmentName,
            ProfileCount = this.ProfileCount,
            BandCount = this.BandCount,
            HorizontalScale = this.HorizontalScale,
            VerticalScale = this.VerticalScale,
            VerticalExaggeration = this.VerticalExaggeration,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Properties: {this.Name} (Sta: {this.StationStart:F2} - {this.StationEnd:F2}, Elev: {this.ElevationMin:F2} - {this.ElevationMax:F2})";
    }
}
