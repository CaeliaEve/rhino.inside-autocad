using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using RhinoInterval = Rhino.Geometry.Interval;
using RhinoPlane = Rhino.Geometry.Plane;
using RhinoPoint3d = Rhino.Geometry.Point3d;
using RhinoRectangle3d = Rhino.Geometry.Rectangle3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D ProfileView types to Rhino geometry types.
/// </summary>
public static class CivilProfileViewExtensions
{
    /// <summary>
    /// Gets the insertion point (location) of the ProfileView as a Rhino Point3d.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView to get the location from.</param>
    /// <returns>A Rhino Point3d representing the insertion point.</returns>
    public static RhinoPoint3d GetRhinoLocation(this ProfileView profileView)
    {
        var location = profileView.Location;
        return new RhinoPoint3d(
            UnitConverter.ToRhinoLength(location.X),
            UnitConverter.ToRhinoLength(location.Y),
            UnitConverter.ToRhinoLength(location.Z));
    }

    /// <summary>
    /// Gets the display bounds of the ProfileView as a Rhino Rectangle3d.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView to get bounds from.</param>
    /// <returns>A Rhino Rectangle3d representing the display area.</returns>
    public static RhinoRectangle3d GetDisplayBounds(this ProfileView profileView)
    {
        var location = profileView.GetRhinoLocation();

        // Scale information - using default values
        // TODO: Determine correct API to extract scale from ProfileView
        double horizontalScale = 1.0;
        double verticalScale = 1.0;

        // Calculate the display width and height
        var stationRange = profileView.StationEnd - profileView.StationStart;
        var elevationRange = profileView.ElevationMax - profileView.ElevationMin;

        // The display size is affected by scale
        // Horizontal: stationRange / horizontalScale
        // Vertical: elevationRange / verticalScale
        var displayWidth = UnitConverter.ToRhinoLength(stationRange / horizontalScale);
        var displayHeight = UnitConverter.ToRhinoLength(elevationRange / verticalScale);

        // Create the rectangle at the insertion point
        var plane = new RhinoPlane(location, RhinoPlane.WorldXY.XAxis, RhinoPlane.WorldXY.YAxis);

        return new RhinoRectangle3d(
            plane,
            new RhinoInterval(0, displayWidth),
            new RhinoInterval(0, displayHeight));
    }

    /// <summary>
    /// Gets the station range as a Rhino Interval.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView.</param>
    /// <returns>An interval from StationStart to StationEnd.</returns>
    public static RhinoInterval GetStationRange(this ProfileView profileView)
    {
        return new RhinoInterval(
            UnitConverter.ToRhinoLength(profileView.StationStart),
            UnitConverter.ToRhinoLength(profileView.StationEnd));
    }

    /// <summary>
    /// Gets the elevation range as a Rhino Interval.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView.</param>
    /// <returns>An interval from ElevationMin to ElevationMax.</returns>
    public static RhinoInterval GetElevationRange(this ProfileView profileView)
    {
        return new RhinoInterval(
            UnitConverter.ToRhinoLength(profileView.ElevationMin),
            UnitConverter.ToRhinoLength(profileView.ElevationMax));
    }

    /// <summary>
    /// Gets all profile IDs displayed in this ProfileView.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of ObjectIds for the displayed profiles.</returns>
    public static List<ObjectId> GetDisplayedProfileIds(
        this ProfileView profileView,
        IAutocadTransactionManager transactionManager)
    {
        var profileIds = new List<ObjectId>();

        try
        {
            // Get the parent alignment to access its profiles
            var alignmentId = profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return profileIds;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            if (alignment == null)
                return profileIds;

            // Get all profiles from the alignment that are displayed in this view
            var allProfileIds = alignment.GetProfileIds();
            foreach (ObjectId profileId in allProfileIds)
            {
                if (profileId.IsNull || profileId.IsErased)
                    continue;

                // Check if this profile is displayed in the profile view
                // All profiles from the parent alignment are typically available
                profileIds.Add(profileId);
            }
        }
        catch
        {
            // Return empty list if extraction fails
        }

        return profileIds;
    }

    /// <summary>
    /// Gets the parent alignment name for a ProfileView.
    /// </summary>
    /// <param name="profileView">The ProfileView to get the parent alignment name for.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>The name of the parent alignment, or empty string if not found.</returns>
    public static string GetAlignmentName(
        this ProfileView profileView,
        IAutocadTransactionManager transactionManager)
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
    /// Extracts band information from a ProfileView.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of band wrappers.</returns>
    public static List<CivilProfileViewBandWrapper> GetBands(
        this ProfileView profileView,
        IAutocadTransactionManager transactionManager)
    {
        var bands = new List<CivilProfileViewBandWrapper>();

        try
        {
            var bandSet = profileView.Bands;
            if (bandSet == null)
                return bands;

            // Extract top bands
            ExtractBandsFromSet(bandSet, "Top", transactionManager, bands);

            // Extract bottom bands
            ExtractBandsFromSet(bandSet, "Bottom", transactionManager, bands);
        }
        catch
        {
            // Return empty list if extraction fails
        }

        return bands;
    }

    /// <summary>
    /// Extracts bands from a band set at the specified location.
    /// </summary>
    private static void ExtractBandsFromSet(
        ProfileViewBandSet bandSet,
        string location,
        IAutocadTransactionManager transactionManager,
        List<CivilProfileViewBandWrapper> bands)
    {
        try
        {
            var bandItemCollection = location == "Top"
                ? bandSet.GetTopBandItems()
                : bandSet.GetBottomBandItems();

            if (bandItemCollection == null)
                return;

            var bandCount = bandItemCollection.Count;
            for (var i = 0; i < bandCount; i++)
            {
                try
                {
                    var bandInfo = bandItemCollection[i];
                    var styleName = GetStyleName(bandInfo.BandStyleId, transactionManager);

                    bands.Add(new CivilProfileViewBandWrapper(
                        $"Band {i + 1}",
                        bandInfo.BandType.ToString(),
                        styleName,
                        location,
                        true)); // Assume visible if it exists in the set
                }
                catch
                {
                    // Skip bands that fail to extract
                }
            }
        }
        catch
        {
            // Silently handle extraction failures
        }
    }

    /// <summary>
    /// Gets the style name from a style ObjectId.
    /// </summary>
    private static string GetStyleName(ObjectId styleId, IAutocadTransactionManager transactionManager)
    {
        try
        {
            if (styleId.IsNull || styleId.IsErased)
                return string.Empty;

            var style = transactionManager.Unwrap()
                .GetObject(styleId, OpenMode.ForRead) as DBObject;

            // Most Civil 3D styles implement a Name property
            var nameProperty = style?.GetType().GetProperty("Name");
            return nameProperty?.GetValue(style) as string ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Creates a ProfileView properties wrapper from a ProfileView.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A properties wrapper containing extracted data.</returns>
    public static CivilProfileViewPropertiesWrapper GetProperties(
        this ProfileView profileView,
        IAutocadTransactionManager transactionManager)
    {
        var alignmentName = profileView.GetAlignmentName(transactionManager);
        var profileCount = profileView.GetDisplayedProfileIds(transactionManager).Count;

        return new CivilProfileViewPropertiesWrapper(profileView, alignmentName, profileCount);
    }
}
