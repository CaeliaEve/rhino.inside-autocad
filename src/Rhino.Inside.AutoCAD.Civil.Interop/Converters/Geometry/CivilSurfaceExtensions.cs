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

        var vertexIndexMap = new Dictionary<CadPoint3d, int>(CadPoint3dComparer.Instance);

        foreach (var vertex in surface.Vertices)
        {
            var location = vertex.Location;
            if (vertexIndexMap.ContainsKey(location))
                continue;

            var index = rhinoMesh.Vertices.Count;
            rhinoMesh.Vertices.Add(location.ToRhinoPoint3d());
            vertexIndexMap[location] = index;
        }

        foreach (var triangle in surface.GetTriangles(false))
        {
            var indexA = vertexIndexMap[triangle.Vertex1.Location];
            var indexB = vertexIndexMap[triangle.Vertex2.Location];
            var indexC = vertexIndexMap[triangle.Vertex3.Location];
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
    public static RhinoMesh ToRhinoMesh(this CivilSurface surface, IAutocadTransaction transactionManager)
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

        using var transactionManagerWrapper = new AutocadTransactionWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = surface.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }
}
