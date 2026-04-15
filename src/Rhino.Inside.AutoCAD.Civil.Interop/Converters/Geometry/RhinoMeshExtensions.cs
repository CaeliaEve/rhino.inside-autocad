using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using CivilTinSurface = Autodesk.Civil.DatabaseServices.TinSurface;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Rhino mesh types to Civil 3D mesh types.
/// </summary>
public static class RhinoMeshExtensions
{

    /// <summary>
    /// Converts a Rhino Mesh to an AutoCAD PolyFaceMesh, applying unit conversion.
    /// Uses the current active document for transaction management.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <returns>A Civil3d TIN Surface with vertices scaled to AutoCAD units.</returns>
    public static CivilTinSurface? ToTinSurface(this RhinoMesh mesh)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var database = activeDocument.Database;

        using var transactionManagerWrapper = new AutocadTransactionWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = mesh.ToTinSurface(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts a Rhino Mesh to a Civil 3D TIN Surface, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Civil 3D TIN Surface with vertices scaled to AutoCAD units.</returns>
    public static CivilTinSurface? ToTinSurface(this RhinoMesh mesh, IAutocadTransaction transactionManager)
    {
        return mesh.ToTinSurface(transactionManager, "ExampleTINSurface", null);
    }

    /// <summary>
    /// Converts a Rhino Mesh to a Civil 3D TIN Surface with a specified name and optional style.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <param name="surfaceName">The name for the new TIN Surface.</param>
    /// <param name="styleId">Optional ObjectId of the surface style to apply. If null, uses Civil 3D default style.</param>
    /// <returns>A Civil 3D TIN Surface with vertices scaled to AutoCAD units, or null if creation fails.</returns>
    public static CivilTinSurface? ToTinSurface(
        this RhinoMesh mesh,
        IAutocadTransaction transactionManager,
        string surfaceName,
        ObjectId? styleId = null)
    {
        try
        {
            var database = transactionManager.Database.Unwrap();

            var surfaceId = TinSurface.Create(database, surfaceName);

            var surface = surfaceId.GetObject(OpenMode.ForWrite) as TinSurface;

            if (surface == null)
            {
                LoggerService.Instance?.LogMessage("Failed to create TIN Surface");
                return null;
            }

            // Apply style if provided
            if (styleId.HasValue && styleId.Value.IsValid && !styleId.Value.IsNull)
            {
                surface.StyleId = styleId.Value;
            }

            var points = new Point3dCollection();

            foreach (var vertex in mesh.Vertices)
            {
                var cadVertex = vertex.ToAutocadPoint3d();

                points.Add(cadVertex);
            }

            surface.AddVertices(points);

            return surface;

        }
        catch (System.Exception ex)
        {
            LoggerService.Instance?.LogError(ex, $"Civil TINSurface ToTinSurface(RhinoMesh mesh, surfaceName: {surfaceName})");
        }

        return null;
    }
}
