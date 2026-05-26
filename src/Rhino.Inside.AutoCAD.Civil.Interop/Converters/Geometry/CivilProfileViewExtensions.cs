using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Diagnostics;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using CivilProfileView = Autodesk.Civil.DatabaseServices.ProfileView;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D ProfileView types to Rhino geometry types.
/// </summary>
public static class CivilProfileViewExtensions
{
    /// <summary>
    /// Converts a Civil 3D ProfileView into Rhino geometry by exploding the ProfileView and converting
    /// the resulting entities into Rhino types. This method extracts all geometric components of the ProfileView.
    /// </summary>
    public static IProfileViewGeometry ToRhinoGeometry(this CivilProfileView profileView)
    {
        IAutocadTransactionManager transactionManager;

        if (profileView.Database != null)
        {
            var fingerprint = profileView.Database.FingerprintGuid;

            var document =
                RhinoInsideAutoCadExtension.Application.RhinoInsideManager.AutoCadInstance
                    .FindDocumentByFingerprintGuid(fingerprint);

            if (document == null)
            {
                return new ProfileViewGeometry();
            }

            transactionManager = document.CreateTransactionManager();
        }
        else
        {
            var activeDocument = Application.DocumentManager.MdiActiveDocument;

            if (activeDocument == null)
            {
                return new ProfileViewGeometry();
            }

            transactionManager = new AutocadTransactionManagerWrapper(activeDocument);
        }

        return transactionManager.PerformTask(() =>
        {
            var result = new ProfileViewGeometry();

            using var explodedObjects = new DBObjectCollection();

            profileView.Explode(explodedObjects);

            foreach (DBObject obj in explodedObjects)
            {
                ProcessExplodedObject(obj, result);
            }

            // Extract profile data curves using Civil 3D coordinate conversion
            ExtractProfileDataCurves(profileView, transactionManager, result);

            return result;

        }, true);

    }
    /// <summary>
    /// Extracts profile data curves from the ProfileView by converting profile geometry
    /// from station-elevation space to ProfileView model space coordinates.
    /// </summary>
    private static void ExtractProfileDataCurves(
        CivilProfileView profileView,
        IAutocadTransactionManager transactionManager,
        IProfileViewGeometry result)
    {
        try
        {
            var alignmentId = profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            if (alignment == null)
                return;

            var profileIds = alignment.GetProfileIds();
            foreach (ObjectId profileId in profileIds)
            {
                if (profileId.IsNull || profileId.IsErased)
                    continue;

                var profile = transactionManager.Unwrap()
                    .GetObject(profileId, OpenMode.ForRead) as Profile;

                if (profile == null)
                    continue;

                var profileCurve = ConvertProfileToProfileViewSpace(profile, profileView);
                if (profileCurve != null)
                    result.ProfileCurves.Add(profileCurve);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting profile data curves: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts a profile curve from station-elevation space to ProfileView model space
    /// by sampling points along the profile and using Civil 3D's FindXYAtStationAndElevation
    /// to transform each point.
    /// </summary>
    private static RhinoCurve? ConvertProfileToProfileViewSpace(
        Profile profile,
        CivilProfileView profileView)
    {
        try
        {
            var points = new List<RhinoPoint3d>();

            var startStation = profile.StartingStation;
            var endStation = profile.EndingStation;

            // Sample every ~5 units, with a minimum of 50 samples for smooth curves
            var numSamples = Math.Max(50, (int)((endStation - startStation) / 5.0));

            for (var i = 0; i <= numSamples; i++)
            {
                var t = (double)i / numSamples;
                var station = startStation + (endStation - startStation) * t;

                // Get elevation at this station from the profile
                var elevation = profile.ElevationAt(station);

                // Convert to ProfileView coordinates using Civil 3D API
                double x = 0, y = 0;
                profileView.FindXYAtStationAndElevation(station, elevation, ref x, ref y);

                // Apply unit conversion from AutoCAD to Rhino units
                var rhinoX = UnitConverter.ToRhinoLength(x);
                var rhinoY = UnitConverter.ToRhinoLength(y);

                points.Add(new RhinoPoint3d(rhinoX, rhinoY, 0));
            }

            if (points.Count < 2)
                return null;

            // Create interpolated curve through the sampled points
            return RhinoCurve.CreateInterpolatedCurve(points, 3);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error converting profile to ProfileView space: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Recursively processes an exploded object and extracts geometry into the result.
    /// Handles nested BlockReferences by exploding them and processing their contents.
    /// </summary>
    private static void ProcessExplodedObject(DBObject obj, ProfileViewGeometry result)
    {
        try
        {
            switch (obj)
            {
                case BlockReference blockRef:
                    using (var nestedObjects = new DBObjectCollection())
                    {
                        blockRef.Explode(nestedObjects);

                        foreach (DBObject nestedObj in nestedObjects)
                        {
                            ProcessExplodedObject(nestedObj, result);
                        }
                    }
                    break;

                case CadCurve acadCurve:
                    var rhinoCurve = acadCurve.ToRhinoCurve();

                    if (rhinoCurve != null)
                        result.GraphCurves.Add(rhinoCurve);

                    break;

                case DBText dbText:
                    var meText = dbText.ConvertToMText();

                    var rhinoText = meText.ToRhinoTextEntity();

                    result.TextEntities.Add(rhinoText);

                    break;

                case MText mText:
                    var rhinoMText = mText.ToRhinoTextEntity();

                    result.TextEntities.Add(rhinoMText);

                    break;
                default:
                    Debug.WriteLine(obj.GetType().Name);
                    break;
            }
        }
        finally
        {
            obj.Dispose();
        }
    }
}
