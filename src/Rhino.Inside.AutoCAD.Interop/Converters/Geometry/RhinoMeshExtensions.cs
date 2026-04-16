using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using CadPoint3dCollection = Autodesk.AutoCAD.Geometry.Point3dCollection;
using CadPolyFaceMesh = Autodesk.AutoCAD.DatabaseServices.PolyFaceMesh;
using CadPolygonMesh = Autodesk.AutoCAD.DatabaseServices.PolygonMesh;
using CadPolyMeshType = Autodesk.AutoCAD.DatabaseServices.PolyMeshType;
using CadSubDMesh = Autodesk.AutoCAD.DatabaseServices.SubDMesh;
using RhinoMesh = Rhino.Geometry.Mesh;
using RhinoNurbsSurface = Rhino.Geometry.NurbsSurface;
using RhinoSubD = Rhino.Geometry.SubD;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Rhino mesh types to AutoCAD mesh types.
/// </summary>
public static class RhinoMeshExtensions
{
    /// <summary>
    /// Converts a Rhino SubD to an AutoCAD SubDMesh, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The Rhino SubD to convert.</param>
    /// <returns>An AutoCAD SubDMesh with vertices scaled to AutoCAD units.</returns>
    public static CadSubDMesh ToAutocadSubDMesh(this RhinoSubD mesh)
    {
        var pointsCollection = new CadPoint3dCollection();
        var vertexMap = new List<SubDVertex>();
        var faceArray = new Int32Collection();

        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var vertex = mesh.Vertices.Find(i);
            var cadPoint = vertex.ControlNetPoint.ToAutocadPoint3d();

            pointsCollection.Add(cadPoint);
            vertexMap.Add(vertex);
        }

        foreach (var face in mesh.Faces)
        {
            var numberOfVertices = face.VertexCount;
            faceArray.Add(numberOfVertices);

            for (var i = 0; i < numberOfVertices; i++)
            {
                var vertex = face.VertexAt(i);
                var index = vertexMap.IndexOf(vertex);
                faceArray.Add(index);
            }
        }

        var subDMesh = new CadSubDMesh();
        subDMesh.SetDatabaseDefaults();
        subDMesh.SetSubDMesh(pointsCollection, faceArray, 0);

        return subDMesh;
    }

    /// <summary>
    /// Converts a Rhino Mesh to an AutoCAD SubDMesh, applying unit conversion.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <returns>An AutoCAD SubDMesh with vertices scaled to AutoCAD units.</returns>
    public static CadSubDMesh ToAutocadSubDMesh(this RhinoMesh mesh)
    {
        var pointsCollection = new CadPoint3dCollection();
        var faceArray = new Int32Collection();

        foreach (var point in mesh.Vertices)
            pointsCollection.Add(point.ToAutocadPoint3d());

        foreach (var face in mesh.Faces)
        {
            faceArray.Add(face.IsQuad ? 4 : 3);
            faceArray.Add(face.A);
            faceArray.Add(face.B);
            faceArray.Add(face.C);

            if (face.IsQuad)
            {
                faceArray.Add(face.D);
            }
        }

        var subDMesh = new CadSubDMesh();
        subDMesh.SetDatabaseDefaults();
        subDMesh.SetSubDMesh(pointsCollection, faceArray, 0);

        return subDMesh;
    }

    /// <summary>
    /// Converts a Rhino NurbsSurface to an AutoCAD PolygonMesh, applying unit conversion.
    /// Only supports surfaces with the same order in U and V.
    /// </summary>
    /// <param name="nurbsSurface">The Rhino NurbsSurface to convert.</param>
    /// <returns>An AutoCAD PolygonMesh with control points scaled to AutoCAD units.</returns>
    /// <exception cref="NotSupportedException">Thrown when the surface has different orders in U and V.</exception>
    public static CadPolygonMesh ToAutocadPolygonMesh(this RhinoNurbsSurface nurbsSurface)
    {
        if (nurbsSurface.OrderU != nurbsSurface.OrderV)
        {
            throw new NotSupportedException("Only surfaces with the same order in U and V are supported.");
        }

        var polygonMeshType = CadPolyMeshType.SimpleMesh;

        switch (nurbsSurface.OrderU - 1)
        {
            case 2:
                polygonMeshType = CadPolyMeshType.BezierSurfaceMesh;
                break;
            case 3:
                polygonMeshType = CadPolyMeshType.CubicSurfaceMesh;
                break;
            case 4:
                polygonMeshType = CadPolyMeshType.QuadSurfaceMesh;
                break;
            default:
                break;
        }

        var pointCollection = new CadPoint3dCollection();

        foreach (var point in nurbsSurface.Points)
        {
            pointCollection.Add(point.Location.ToAutocadPoint3d());
        }

        var polygonMesh = new CadPolygonMesh(polygonMeshType, nurbsSurface.Points.CountU,
            nurbsSurface.Points.CountV, pointCollection, nurbsSurface.IsClosed(1),
            nurbsSurface.IsClosed(0));

        return polygonMesh;
    }

    /// <summary>
    /// Converts a Rhino Mesh to an AutoCAD PolyFaceMesh, applying unit conversion.
    /// Uses the current active document for transaction management.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <returns>An AutoCAD PolyFaceMesh with vertices scaled to AutoCAD units.</returns>
    public static CadPolyFaceMesh? ToAutocadPolyFaceMesh(this RhinoMesh mesh)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var database = activeDocument.Database;

        var transactionManagerWrapper = new AutocadTransactionWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = mesh.ToAutocadPolyFaceMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts a Rhino Mesh to an AutoCAD PolyFaceMesh, applying unit conversion.
    /// AutoCAD mesh faces use 1-based indexing, so indices are adjusted accordingly.
    /// </summary>
    /// <param name="mesh">The Rhino Mesh to convert.</param>
    /// <param name="autocadTransaction">The transaction manager for database operations.</param>
    /// <returns>An AutoCAD PolyFaceMesh with vertices scaled to AutoCAD units.</returns>
    /// <remarks>
    /// This does not work in Civil 3d because it does not support PolyFaceMesh. Use
    /// ToAutocadSubDMesh instead for better compatibility.
    /// </remarks>
    public static CadPolyFaceMesh? ToAutocadPolyFaceMesh(this RhinoMesh mesh,
        IAutocadTransaction autocadTransaction)
    {
        var polyFaceMesh = new CadPolyFaceMesh();
        var clone = new CadPolyFaceMesh();

        try
        {
            var transactionManager = autocadTransaction.Unwrap();
            var transaction = transactionManager.TopTransaction;

            if (transaction == null)
                throw new InvalidOperationException("No active transaction available for PolyFaceMesh creation");

            var blockTable = transaction.GetObject(autocadTransaction.BlockTableId.Unwrap(), OpenMode.ForRead) as BlockTable;

            var blockTableRecord = transaction.GetObject(blockTable![BlockTableRecord.ModelSpace],
                OpenMode.ForWrite) as BlockTableRecord;

            blockTableRecord!.AppendEntity(polyFaceMesh);

            transaction.AddNewlyCreatedDBObject(polyFaceMesh, true);

            foreach (var point in mesh.Vertices)
            {
                var vertex = new PolyFaceMeshVertex(point.ToAutocadPoint3d());

                polyFaceMesh.AppendVertex(vertex);
                transaction.AddNewlyCreatedDBObject(vertex, true);
            }

            foreach (var face in mesh.Faces)
            {
                if (face.IsQuad)
                {
                    var quadFaceRecord = new FaceRecord((short)(face.A + 1), (short)(face.B + 1), (short)(face.C + 1), (short)(face.D + 1));

                    polyFaceMesh.AppendFaceRecord(quadFaceRecord);

                    transaction.AddNewlyCreatedDBObject(quadFaceRecord, true);

                    continue;
                }

                var faceRecord = new FaceRecord((short)(face.A + 1), (short)(face.B + 1), (short)(face.C + 1), 0);

                polyFaceMesh.AppendFaceRecord(faceRecord);

                transaction.AddNewlyCreatedDBObject(faceRecord, true);
            }

            clone = polyFaceMesh.Clone() as CadPolyFaceMesh;

            polyFaceMesh.Erase(true);
        }
        catch (System.Exception ex)
        {
            LoggerService.Instance?.LogError(ex, "AutoCAD PolyFaceMesh ToAutocadPolyFaceMesh(RhinoMesh mesh)");
        }

        return clone;
    }
}
