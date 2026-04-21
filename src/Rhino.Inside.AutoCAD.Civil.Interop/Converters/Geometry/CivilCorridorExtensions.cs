using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoMesh = Rhino.Geometry.Mesh;
using RhinoPoint3d = Rhino.Geometry.Point3d;
using RhinoPolylineCurve = Rhino.Geometry.PolylineCurve;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D Corridor types to Rhino geometry types.
/// </summary>
public static class CivilCorridorExtensions
{
    /// <summary>
    /// Extracts all baselines from a Civil 3D Corridor as wrapper objects.
    /// </summary>
    /// <param name="corridor">The Civil 3D Corridor to extract baselines from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of baseline wrappers.</returns>
    public static List<CivilCorridorBaselineWrapper> GetBaselines(
        this Corridor corridor,
        IAutocadTransactionManager transactionManager)
    {
        var baselines = new List<CivilCorridorBaselineWrapper>();

        try
        {
            foreach (var baseline in corridor.Baselines)
            {
                var wrapper = new CivilCorridorBaselineWrapper(baseline);
                baselines.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if baseline extraction fails
        }

        return baselines;
    }

    /// <summary>
    /// Extracts all regions from a Civil 3D Corridor Baseline as wrapper objects.
    /// </summary>
    /// <param name="baseline">The baseline to extract regions from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of baseline region wrappers.</returns>
    public static List<CivilCorridorBaselineRegionWrapper> GetRegions(
        this Baseline baseline,
        IAutocadTransactionManager transactionManager)
    {
        var regions = new List<CivilCorridorBaselineRegionWrapper>();

        try
        {
            foreach (var region in baseline.BaselineRegions)
            {
                var wrapper = new CivilCorridorBaselineRegionWrapper(region);
                regions.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if region extraction fails
        }

        return regions;
    }

    /// <summary>
    /// Extracts all corridor surfaces from a Civil 3D Corridor as wrapper objects.
    /// </summary>
    /// <param name="corridor">The Civil 3D Corridor to extract surfaces from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of corridor surface wrappers.</returns>
    public static List<CivilCorridorSurfaceWrapper> GetCorridorSurfaces(
        this Corridor corridor,
        IAutocadTransactionManager transactionManager)
    {
        var surfaces = new List<CivilCorridorSurfaceWrapper>();

        try
        {
            var transaction = transactionManager.Unwrap();

            foreach (var surface in corridor.CorridorSurfaces)
            {
                RhinoMesh? mesh = null;

                // Try to get the actual TIN surface and convert to mesh
                if (!surface.SurfaceId.IsNull && !surface.SurfaceId.IsErased)
                {
                    try
                    {
                        var tinSurface = transaction.GetObject(surface.SurfaceId, OpenMode.ForRead) as TinSurface;
                        if (tinSurface != null)
                        {
                            mesh = tinSurface.ToRhinoMesh(transactionManager);
                        }
                    }
                    catch
                    {
                        // Mesh extraction failed, wrapper will have null mesh
                    }
                }

                var wrapper = new CivilCorridorSurfaceWrapper(surface, mesh);
                surfaces.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if surface extraction fails
        }

        return surfaces;
    }

    /// <summary>
    /// Extracts all feature lines from a Civil 3D Corridor Baseline as wrapper objects.
    /// </summary>
    /// <param name="baseline">The baseline to extract feature lines from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of feature line wrappers.</returns>
    public static List<CivilCorridorFeatureLineWrapper> GetFeatureLines(
        this Baseline baseline,
        IAutocadTransactionManager transactionManager)
    {
        var featureLines = new List<CivilCorridorFeatureLineWrapper>();

        try
        {
            var featureLineCollection = baseline.MainBaselineFeatureLines;

            foreach (var lineCollection in featureLineCollection.FeatureLineCollectionMap)
            {
                var codeName = lineCollection.FeatureLineCodeInfo.CodeName ?? "Unknown";

                foreach (var featureLine in lineCollection)
                {
                    var curve = featureLine.ToRhinoCurve(transactionManager);
                    if (curve != null)
                    {
                        var wrapper = new CivilCorridorFeatureLineWrapper(featureLine, codeName, curve);
                        featureLines.Add(wrapper);
                    }
                }
            }
        }
        catch
        {
            // Return empty list if feature line extraction fails
        }

        return featureLines;
    }

    /// <summary>
    /// Converts a Civil 3D CorridorFeatureLine to a Rhino Curve.
    /// </summary>
    /// <param name="featureLine">The corridor feature line to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Curve representing the feature line geometry.</returns>
    public static RhinoCurve? ToRhinoCurve(
        this CorridorFeatureLine featureLine,
        IAutocadTransactionManager transactionManager)
    {
        try
        {
            var points = new List<RhinoPoint3d>();

            foreach (var point in featureLine.FeatureLinePoints)
            {
                var rhinoPoint = point.XYZ.ToRhinoPoint3d();
                points.Add(rhinoPoint);
            }

            if (points.Count < 2)
                return null;

            return new RhinoPolylineCurve(points);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a Civil 3D CorridorSurface to a Rhino Mesh.
    /// </summary>
    /// <param name="surface">The corridor surface to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh representing the corridor surface.</returns>
    public static RhinoMesh? ToRhinoMesh(
        this CorridorSurface surface,
        IAutocadTransactionManager transactionManager)
    {
        if (surface.SurfaceId.IsNull || surface.SurfaceId.IsErased)
            return null;

        try
        {
            var transaction = transactionManager.Unwrap();
            var tinSurface = transaction.GetObject(surface.SurfaceId, OpenMode.ForRead) as TinSurface;

            if (tinSurface == null)
                return null;

            return tinSurface.ToRhinoMesh(transactionManager);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a Civil 3D Corridor to a combined Rhino Mesh from all corridor surfaces.
    /// </summary>
    /// <param name="corridor">The corridor to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A combined Rhino Mesh from all corridor surfaces.</returns>
    public static RhinoMesh ToRhinoMesh(
        this Corridor corridor,
        IAutocadTransactionManager transactionManager)
    {
        var combinedMesh = new RhinoMesh();

        try
        {
            foreach (var surface in corridor.CorridorSurfaces)
            {
                var surfaceMesh = surface.ToRhinoMesh(transactionManager);
                if (surfaceMesh != null)
                {
                    combinedMesh.Append(surfaceMesh);
                }
            }
        }
        catch
        {
            // Return empty mesh if conversion fails
        }

        return combinedMesh;
    }

    /// <summary>
    /// Converts a Civil 3D Corridor to a Rhino Mesh using the current active document.
    /// </summary>
    /// <param name="corridor">The corridor to convert.</param>
    /// <returns>A combined Rhino Mesh from all corridor surfaces.</returns>
    public static RhinoMesh ToRhinoMesh(this Corridor corridor)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var transactionManagerWrapper = new AutocadTransactionManagerWrapper(activeDocument);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = corridor.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }
}
