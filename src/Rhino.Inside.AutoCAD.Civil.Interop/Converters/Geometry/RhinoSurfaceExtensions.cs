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
/// Provides extension methods for converting Rhino surface/mesh types to AutoCAD surface/mesh types.
/// </summary>
public static class RhinoSurfaceExtensions
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

        using var transactionManagerWrapper = new TransactionManagerWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = mesh.ToTinSurface(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts a Rhino Mesh to an AutoCAD PolyFaceMesh, applying unit conversion.
    /// AutoCAD mesh faces use 1-based indexing, so indices are adjusted accordingly.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>An AutoCAD PolyFaceMesh with vertices scaled to AutoCAD units.</returns>
    public static CivilTinSurface? ToTinSurface(this RhinoMesh mesh, ITransactionManager transactionManager)
    {
        try
        {
            var surfaceName = "ExampleTINSurface";

            var surfaceId = TinSurface.Create(transactionManager.Database.Unwrap(), surfaceName);

            var surface = surfaceId.GetObject(OpenMode.ForWrite) as TinSurface;

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
            LoggerService.Instance?.LogError(ex, "Civil TINSurface ToTinSurface(RhinoMesh mesh)");
        }

        return null;
    }
}
