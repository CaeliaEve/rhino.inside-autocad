using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using CadObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using CadPolyFaceMesh = Autodesk.AutoCAD.DatabaseServices.PolyFaceMesh;
using CadPolygonMesh = Autodesk.AutoCAD.DatabaseServices.PolygonMesh;
using CadPolygonMeshVertex = Autodesk.AutoCAD.DatabaseServices.PolygonMeshVertex;
using CadPolyMeshType = Autodesk.AutoCAD.DatabaseServices.PolyMeshType;
using CadSubDMesh = Autodesk.AutoCAD.DatabaseServices.SubDMesh;
using CadVertex3dType = Autodesk.AutoCAD.DatabaseServices.Vertex3dType;
using RhinoMesh = Rhino.Geometry.Mesh;
using RhinoNurbsSurface = Rhino.Geometry.NurbsSurface;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting AutoCAD mesh types to Rhino mesh types.
/// </summary>
public static class AutocadMeshExtensions
{
    /// <summary>
    /// Converts an AutoCAD SubDMesh to a Rhino Mesh, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The AutoCAD SubDMesh to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CadSubDMesh mesh)
    {
        var rhinoMesh = new RhinoMesh();

        foreach (CadPoint3d point in mesh.Vertices)
            rhinoMesh.Vertices.Add(point.ToRhinoPoint3d());

        var edges = 0;

        for (var x = 0; x < mesh.FaceArray.Count; x = x + edges + 1)
        {
            edges = mesh.FaceArray[x];

            var faces = new List<int>();

            for (var y = x + 1; y <= x + edges; y++)
            {
                var faceInt = mesh.FaceArray[y];
                faces.Add(faceInt);
            }

            if (faces.Count == 4)
            {
                rhinoMesh.Faces.AddFace(faces[0], faces[1], faces[2], faces[3]);
                continue;
            }
            rhinoMesh.Faces.AddFace(faces[0], faces[1], faces[2]);
        }

        return rhinoMesh;
    }

    /// <summary>
    /// Converts an AutoCAD PolygonMesh to a Rhino NurbsSurface, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The AutoCAD PolygonMesh to convert.</param>
    /// <returns>A Rhino NurbsSurface with control points scaled to Rhino units.</returns>
    public static RhinoNurbsSurface ToRhinoNurbsSurface(this CadPolygonMesh mesh)
    {
        var degree = 1;
        switch (mesh.PolyMeshType)
        {
            case CadPolyMeshType.BezierSurfaceMesh:
                degree = 2;
                break;
            case CadPolyMeshType.CubicSurfaceMesh:
                degree = 3;
                break;
            case CadPolyMeshType.QuadSurfaceMesh:
                degree = 4;
                break;
            default:
                break;
        }

        var controlPointsU = mesh.MSize;
        var controlPointsV = mesh.NSize;

        var points = new List<RhinoPoint3d>();

        foreach (var meshItem in mesh)
        {
            if (meshItem is not CadPolygonMeshVertex vertex || vertex.VertexType != CadVertex3dType.ControlVertex)
                continue;

            var convertedPoint = vertex.Position.ToRhinoPoint3d();
            points.Add(convertedPoint);
        }

        var rhinoSurface = RhinoNurbsSurface.CreateFromPoints(points, controlPointsU,
            controlPointsV, degree, degree);

        return rhinoSurface;
    }

    /// <summary>
    /// Converts an AutoCAD PolyFaceMesh to a Rhino Mesh, applying unit conversion.
    /// Uses the current active document for transaction management.
    /// </summary>
    /// <param name="mesh">The AutoCAD PolyFaceMesh to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CadPolyFaceMesh mesh)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var transactionManagerWrapper = new AutocadTransactionManagerWrapper(activeDocument);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = mesh.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts an AutoCAD PolyFaceMesh to a Rhino Mesh, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The AutoCAD PolyFaceMesh to convert.</param>
    /// <param name="autocadTransaction">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CadPolyFaceMesh mesh, IAutocadTransactionManager autocadTransaction)
    {
        var rhinoMesh = new RhinoMesh();

        try
        {
            var transaction = autocadTransaction.Unwrap();

            foreach (CadObjectId id in mesh)
            {
                var dbObject = transaction.GetObject(id, OpenMode.ForRead);

                switch (dbObject)
                {
                    case PolyFaceMeshVertex polyFaceMeshVertex:
                        {
                            var rhinoVertex = polyFaceMeshVertex.Position.ToRhinoPoint3d();

                            rhinoMesh.Vertices.Add(rhinoVertex);
                            continue;
                        }
                    case FaceRecord face when face.GetVertexAt(3) != 0:
                        rhinoMesh.Faces.AddFace(face.GetVertexAt(0), face.GetVertexAt(1),
                            face.GetVertexAt(2), face.GetVertexAt(3));
                        continue;

                    case FaceRecord face:
                        rhinoMesh.Faces.AddFace(face.GetVertexAt(0), face.GetVertexAt(1),
                            face.GetVertexAt(2));
                        break;
                }
            }
        }
        catch (System.Exception)
        {
            // Swallow exceptions during conversion
        }

        return rhinoMesh;
    }
}
