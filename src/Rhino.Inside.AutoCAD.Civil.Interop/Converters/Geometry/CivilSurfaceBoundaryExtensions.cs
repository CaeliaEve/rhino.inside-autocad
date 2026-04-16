using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using RhinoPolyline = Rhino.Geometry.Polyline;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for extracting boundary data from Civil 3D TIN Surfaces.
/// </summary>
public static class CivilSurfaceBoundaryExtensions
{
    /// <summary>
    /// Extracts all boundaries from a TIN surface.
    /// </summary>
    /// <param name="surface">The TIN surface to extract boundaries from.</param>
    /// <returns>A list of boundary wrappers containing the extracted boundary data.</returns>
    public static IReadOnlyList<CivilSurfaceBoundaryWrapper> GetBoundaries(this TinSurface surfaceRaw, IAutocadTransactionManager transaction)
    {
        var boundaries = new List<CivilSurfaceBoundaryWrapper>();

        var surface = transaction.Unwrap()
            .GetObject(surfaceRaw.Id, OpenMode.ForWrite) as TinSurface;

        // Extract the outer border of the surface
        var outerBorder = ExtractOuterBorder(surface, transaction);
        if (outerBorder != null)
        {
            boundaries.Add(new CivilSurfaceBoundaryWrapper(0, outerBorder, "Outer Border"));
        }

        // Process defined boundaries from BoundariesDefinition
        // These are user-defined boundaries (Hide, Show, DataClip, etc.)
        var boundariesDefinition = surface.BoundariesDefinition;

        for (var i = 0; i < boundariesDefinition.Count; i++)
        {
            var boundaryOp = boundariesDefinition[i];

            // Get boundary type and name from the operation
            var boundaryTypeInt = ConvertBoundaryType(boundaryOp.BoundaryType);
            var opName = boundaryOp.Name ?? $"BoundaryOp_{i}";

            // SurfaceOperationAddBoundary is a collection of SurfaceBoundary objects
            // Iterate through each SurfaceBoundary in the operation
            for (var j = 0; j < boundaryOp.Count; j++)
            {
                var surfaceBoundary = boundaryOp[j];
                var polyline = ExtractBoundaryVertices(surfaceBoundary);

                if (polyline != null && polyline.Count >= 3)
                {
                    var name = boundaryOp.Count > 1 ? $"{opName}_{j}" : opName;
                    boundaries.Add(new CivilSurfaceBoundaryWrapper(boundaryTypeInt, polyline, name));
                }
            }
        }

        return boundaries;
    }

    /// <summary>
    /// Extracts the outer border of the surface.
    /// </summary>
    private static RhinoPolyline? ExtractOuterBorder(TinSurface surface, IAutocadTransactionManager transactionWrapper)
    {
        try
        {
            var transaction = transactionWrapper.Unwrap();

            // ExtractBorder returns ObjectIdCollection of polyline entities created in the drawing
            var borderIds = surface.ExtractBorder(SurfaceExtractionSettingsType.Model);

            if (borderIds == null || borderIds.Count == 0)
                return null;

            var polyline = new RhinoPolyline();

            foreach (ObjectId id in borderIds)
            {
                var entity = transaction.GetObject(id, OpenMode.ForRead);

                if (entity is Polyline3d polyline3d)
                {
                    // Polyline3d stores vertices as separate PolylineVertex3d objects
                    foreach (ObjectId vertexId in polyline3d)
                    {
                        if (transaction.GetObject(vertexId, OpenMode.ForRead) is PolylineVertex3d vertex)
                        {
                            polyline.Add(vertex.Position.ToRhinoPoint3d());
                        }
                    }
                }
                else if (entity is Polyline pline)
                {
                    for (var i = 0; i < pline.NumberOfVertices; i++)
                    {
                        var pt = pline.GetPoint3dAt(i);
                        polyline.Add(pt.ToRhinoPoint3d());
                    }
                }

                // Optionally erase the extracted entity since we just need the geometry
                entity.UpgradeOpen();
                entity.Erase();
            }

            if (polyline.Count < 3)
                return null;

            // Close the polyline if not already closed
            if (polyline.Count > 0 && polyline.First != polyline.Last)
            {
                polyline.Add(polyline.First);
            }

            return polyline;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the boundary vertices from a SurfaceBoundary.
    /// </summary>
    private static RhinoPolyline? ExtractBoundaryVertices(SurfaceBoundary surfaceBoundary)
    {
        try
        {
            // SurfaceBoundary should have vertices or points
            // Iterate through the boundary to get its vertices
            var polyline = new RhinoPolyline();

            var vertices = surfaceBoundary.Vertices!;

            foreach (CadPoint3d vertex in vertices)
            {
                polyline.Add(vertex.ToRhinoPoint3d());
            }

            if (polyline.Count < 3)
                return null;

            // Close the polyline if not already closed
            if (polyline.Count > 0 && polyline.First != polyline.Last)
            {
                polyline.Add(polyline.First);
            }

            return polyline;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts the Civil 3D SurfaceBoundaryType enum to an integer.
    /// </summary>
    private static int ConvertBoundaryType(SurfaceBoundaryType boundaryType)
    {
        return boundaryType switch
        {
            SurfaceBoundaryType.Outer => 0,
            SurfaceBoundaryType.DataClip => 1,
            SurfaceBoundaryType.Hide => 2,
            SurfaceBoundaryType.Show => 3,
            _ => -1
        };
    }
}
