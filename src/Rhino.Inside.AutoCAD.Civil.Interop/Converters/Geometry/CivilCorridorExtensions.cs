using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
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
