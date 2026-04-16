using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;
using CivilTinSurface = Autodesk.Civil.DatabaseServices.TinSurface;
using CivilTinVolumeSurface = Autodesk.Civil.DatabaseServices.TinVolumeSurface;
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
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CivilSurface surface, IAutocadTransactionManager transactionManager)
    {
        switch (surface)
        {
            case CivilTinSurface tinSurface:
                {
                    return tinSurface.ToRhinoMesh(transactionManager);
                }

            case CivilTinVolumeSurface volumeSurface:
                {
                    return volumeSurface.ToRhinoMesh(transactionManager);
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

        var transactionManagerWrapper = new AutocadTransactionManagerWrapper(activeDocument);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = surface.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts an AutoCAD PolyFaceMesh to a Rhino Mesh, applying unit conversion.
    /// </summary>
    /// <param name="surface">The Civil Surface to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this CivilTinSurface surface, IAutocadTransactionManager transactionManager)
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
    /// Converts a Civil 3D TIN Volume Surface to a Rhino Mesh using a transaction.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this TinVolumeSurface volumeSurface, IAutocadTransactionManager transactionManager)
    {
        var rhinoMesh = new RhinoMesh();

        try
        {
            var volumeSurfaceProperties = volumeSurface.GetVolumeProperties();

            var transaction = transactionManager.Unwrap();

            var baseSurfaceId = volumeSurfaceProperties.BaseSurface;
            var comparisonSurfaceId = volumeSurfaceProperties.ComparisonSurface;

            var baseSurface =
                transaction.GetObject(baseSurfaceId, OpenMode.ForRead) as CivilSurface;

            var comparisonSurface =
                transaction.GetObject(comparisonSurfaceId, OpenMode.ForRead) as CivilSurface;

            var baseRhinoMesh = baseSurface.ToRhinoMesh(transactionManager);

            var comparisonRhinoMesh = comparisonSurface.ToRhinoMesh(transactionManager);

            rhinoMesh.Append(baseRhinoMesh);
            rhinoMesh.Append(comparisonRhinoMesh);

        }
        catch
        {
            // If mesh creation fails, return empty mesh
        }

        return rhinoMesh;
    }

    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a Rhino Mesh using the current active document.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this TinVolumeSurface volumeSurface)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var transactionManagerWrapper = new AutocadTransactionManagerWrapper(activeDocument);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = volumeSurface.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }

    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a VolumeSurfaceAdapter using a transaction.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A VolumeSurfaceAdapter containing separate base and comparison meshes.</returns>
    public static VolumeSurfaceAdapter ToVolumeSurfaceAdapter(this TinVolumeSurface volumeSurface, IAutocadTransactionManager transactionManager)
    {
        try
        {
            var volumeSurfaceProperties = volumeSurface.GetVolumeProperties();

            var transaction = transactionManager.Unwrap();

            var baseSurfaceId = volumeSurfaceProperties.BaseSurface;
            var comparisonSurfaceId = volumeSurfaceProperties.ComparisonSurface;

            var baseSurface =
                transaction.GetObject(baseSurfaceId, OpenMode.ForRead) as CivilSurface;

            var comparisonSurface =
                transaction.GetObject(comparisonSurfaceId, OpenMode.ForRead) as CivilSurface;

            var baseMesh = baseSurface?.ToRhinoMesh(transactionManager);
            var comparisonMesh = comparisonSurface?.ToRhinoMesh(transactionManager);

            return new VolumeSurfaceAdapter(baseMesh, comparisonMesh);
        }
        catch
        {
            // If mesh creation fails, return adapter with null meshes
            return new VolumeSurfaceAdapter(null, null);
        }
    }

    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a VolumeSurfaceAdapter using the current active document.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <returns>A VolumeSurfaceAdapter containing separate base and comparison meshes.</returns>
    public static VolumeSurfaceAdapter ToVolumeSurfaceAdapter(this TinVolumeSurface volumeSurface)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var transactionManagerWrapper = new AutocadTransactionManagerWrapper(activeDocument);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = volumeSurface.ToVolumeSurfaceAdapter(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }
}
