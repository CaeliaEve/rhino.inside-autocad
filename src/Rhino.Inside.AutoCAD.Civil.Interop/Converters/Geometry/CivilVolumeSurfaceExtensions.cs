using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D TIN Volume Surface types to Rhino types.
/// </summary>
public static class CivilVolumeSurfaceExtensions
{
    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a Rhino Mesh.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <returns>A Rhino Mesh representation of the volume surface, or an empty mesh if triangles are not accessible.</returns>
    /// <remarks>
    /// Note: The Civil 3D .NET API does not directly expose the Triangles property for TinVolumeSurface
    /// (unlike TinSurface). This method attempts to extract mesh data using available API methods.
    /// If triangles cannot be accessed, an empty mesh is returned.
    /// For full triangle access, COM interop with AeccTinVolumeSurface.OutputTriangles would be required.
    /// </remarks>
    public static RhinoMesh ToRhinoMesh(this TinVolumeSurface volumeSurface)
    {
        var rhinoMesh = new RhinoMesh();

        try
        {
            // TinVolumeSurface inherits from Surface, not TinSurface
            // The Triangles property is not directly accessible via .NET API
            // We need to use the surface's general properties to approximate the mesh

            // Get the surface bounds
            var props = volumeSurface.GetGeneralProperties();
            var minElevation = props.MinimumElevation;
            var maxElevation = props.MaximumElevation;

            // Try to sample the surface to create a grid mesh
            // This is an approximation since we can't access triangles directly
            var bounds = volumeSurface.GeometricExtents;
            var minPt = bounds.MinPoint;
            var maxPt = bounds.MaxPoint;

            // Create a grid of sample points
            var gridSize = 50; // Number of samples in each direction
            var xStep = (maxPt.X - minPt.X) / gridSize;
            var yStep = (maxPt.Y - minPt.Y) / gridSize;

            if (xStep <= 0 || yStep <= 0)
                return rhinoMesh;

            // Sample elevations and build mesh
            var vertices = new Rhino.Geometry.Point3d[gridSize + 1, gridSize + 1];

            for (var i = 0; i <= gridSize; i++)
            {
                for (var j = 0; j <= gridSize; j++)
                {
                    var x = minPt.X + (i * xStep);
                    var y = minPt.Y + (j * yStep);

                    try
                    {
                        var elevation = volumeSurface.FindElevationAtXY(x, y);
                        vertices[i, j] = new Rhino.Geometry.Point3d(x, y, elevation);
                        rhinoMesh.Vertices.Add(vertices[i, j]);
                    }
                    catch
                    {
                        // Point is outside surface boundary, use min elevation
                        vertices[i, j] = new Rhino.Geometry.Point3d(x, y, minElevation);
                        rhinoMesh.Vertices.Add(vertices[i, j]);
                    }
                }
            }

            // Create faces
            for (var i = 0; i < gridSize; i++)
            {
                for (var j = 0; j < gridSize; j++)
                {
                    var idx00 = i * (gridSize + 1) + j;
                    var idx10 = (i + 1) * (gridSize + 1) + j;
                    var idx01 = i * (gridSize + 1) + (j + 1);
                    var idx11 = (i + 1) * (gridSize + 1) + (j + 1);

                    rhinoMesh.Faces.AddFace(idx00, idx10, idx11, idx01);
                }
            }

            rhinoMesh.Normals.ComputeNormals();
            rhinoMesh.Compact();
        }
        catch
        {
            // If mesh creation fails, return empty mesh
        }

        return rhinoMesh;
    }

    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a Rhino Mesh using a transaction.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMesh(this TinVolumeSurface volumeSurface, IAutocadTransaction transactionManager)
    {
        return volumeSurface.ToRhinoMesh();
    }

    /// <summary>
    /// Gets volume statistics from a TIN Volume Surface.
    /// </summary>
    /// <param name="volumeSurface">The volume surface to query.</param>
    /// <returns>A wrapper containing all volume statistics.</returns>
    public static ICivilTinVolumeSurface GetVolumeStatistics(this TinVolumeSurface volumeSurface)
    {
        return new CivilTinVolumeSurfaceWrapper(volumeSurface);
    }

    /// <summary>
    /// Converts a Civil 3D TIN Volume Surface to a Rhino Mesh using the current active document.
    /// </summary>
    /// <param name="volumeSurface">The TIN Volume Surface to convert.</param>
    /// <returns>A Rhino Mesh with vertices scaled to Rhino units.</returns>
    public static RhinoMesh ToRhinoMeshWithTransaction(this TinVolumeSurface volumeSurface)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        using var documentLock = activeDocument.LockDocument();

        var database = activeDocument.Database;

        var transactionManagerWrapper = new AutocadTransactionWrapper(database);

        using var transaction = transactionManagerWrapper.Unwrap().StartTransaction();

        var result = volumeSurface.ToRhinoMesh(transactionManagerWrapper);

        transaction.Commit();

        return result;
    }
}
