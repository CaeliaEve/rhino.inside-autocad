using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;
using CivilTinSurface = Autodesk.Civil.DatabaseServices.TinSurface;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil surface/mesh types to Rhino surface/mesh types.
/// </summary>
public static class CivilSurfaceExtensions
{
    /// <summary>
    /// Converts an AutoCAD PolyFaceMesh to a Rhino Mesh, applying unit conversion.
    /// </summary>
    /// <param name="surface">The Civil Surface to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this TinSurface surface)
    {
        var rhinoMesh = new RhinoMesh();

        var vertices = surface.Vertices;

        var cadPoints = new HashSet<CadPoint3d>();

        foreach (var vertex in vertices)
        {
            var location = vertex.Location;

            if (cadPoints.Add(location) == false)
            {
                continue;
            }

            var rhinoPoint = location.ToRhinoPoint3d();

            rhinoMesh.Vertices.Add(rhinoPoint);
        }

        var triangles = surface.GetTriangles(false);

        var vertexList = cadPoints.ToList();

        foreach (var surfaceTriangle in triangles)
        {
            var indexA = vertexList.IndexOf(surfaceTriangle.Vertex1.Location);
            var indexB = vertexList.IndexOf(surfaceTriangle.Vertex2.Location);
            var indexC = vertexList.IndexOf(surfaceTriangle.Vertex3.Location);

            rhinoMesh.Faces.AddFace(indexA, indexB, indexC);
        }

        return rhinoMesh;

    }

    /// <summary>
    /// Converts an AutoCAD PolyFaceMesh to a Rhino Mesh, applying unit conversion.
    /// </summary>
    /// <param name="surface">The Civil Surface to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CivilSurface surface, ITransactionManager transactionManager)
    {
        switch (surface)
        {
            case CivilTinSurface tinSurface:
                {
                    return tinSurface.ToRhinoMesh(transactionManager);
                }
            default:
                {
                    throw new System.Exception("Missing Surface conversion");
                }
        }
    }

    /// <summary>
    /// Converts a Civil3d Surface to a Rhino Mesh, applying unit conversion.
    /// Uses the current active document for transaction management.
    /// </summary>
    /// <param name="surface">The AutoCAD PolyFaceMesh to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CivilSurface surface)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var database = activeDocument.Database;

        using var transactionManagerWrapper = new TransactionManagerWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = surface.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }
}
