using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for extracting contour data from Civil 3D TIN Surfaces.
/// </summary>
public static class CivilSurfaceContourExtensions
{
    /// <summary>
    /// Extracts both major and minor contours from a TIN surface using the surface's own settings.
    /// </summary>
    /// <param name="surfaceRaw">The TIN surface to extract contours from.</param>
    /// <param name="transaction">The current AutoCAD transaction.</param>
    /// <returns>A list of contour wrappers containing both major (Type=1) and minor (Type=2) contours.</returns>
    public static IReadOnlyList<CivilSurfaceContourWrapper> GetContours(
        this TinSurface surfaceRaw,
        IAutocadTransactionManager transaction)
    {
        var contours = new List<CivilSurfaceContourWrapper>();

        var transactionManager = transaction.Unwrap();
        var surface = transactionManager.GetObject(surfaceRaw.Id, OpenMode.ForWrite) as TinSurface;

        if (surface == null)
            return contours;

        // Extract major contours (Type=1)
        ExtractContoursOfType(surface, transaction, contours, 1);

        // Extract minor contours (Type=2)
        ExtractContoursOfType(surface, transaction, contours, 2);

        return contours;
    }

    /// <summary>
    /// Extracts contours of a specific type from the surface.
    /// </summary>
    private static void ExtractContoursOfType(
        TinSurface surface,
      IAutocadTransactionManager transaction,
        List<CivilSurfaceContourWrapper> contours,
        int contourType)
    {
        try
        {
            var contourIds = contourType switch
            {
                1 => surface.ExtractMajorContours(SurfaceExtractionSettingsType.Model),
                2 => surface.ExtractMinorContours(SurfaceExtractionSettingsType.Model),
                _ => null
            };

            if (contourIds == null || contourIds.Count == 0)
                return;

            foreach (ObjectId id in contourIds)
            {
                var entity = transaction.Unwrap().GetObject(id, OpenMode.ForRead);

                RhinoCurve? rhinoCurve = null;
                var elevation = 0.0;

                if (entity is Polyline3d polyline3d)
                {
                    rhinoCurve = ConvertPolyline3dToRhinoCurve(polyline3d, transaction, out elevation);
                }
                else if (entity is Polyline pline)
                {
                    rhinoCurve = ConvertPolylineToRhinoCurve(pline, out elevation);
                }
                else if (entity is CadCurve curve)
                {
                    rhinoCurve = curve.ToRhinoCurve();
                    elevation = curve.StartPoint.ToRhinoPoint3d().Z;
                }

                if (rhinoCurve != null)
                {
                    contours.Add(new CivilSurfaceContourWrapper(contourType, rhinoCurve, elevation));
                }

                // Erase the temporary extracted entity
                entity.UpgradeOpen();
                entity.Erase();
            }
        }
        catch
        {
            // If extraction fails for this type, continue with other types
        }
    }

    /// <summary>
    /// Converts an AutoCAD Polyline3d to a Rhino curve.
    /// </summary>
    private static RhinoCurve? ConvertPolyline3dToRhinoCurve(
        Polyline3d polyline3d,
        IAutocadTransactionManager trans,
        out double elevation)
    {
        var points = new List<Rhino.Geometry.Point3d>();
        elevation = 0.0;
        var firstPoint = true;

        foreach (ObjectId vertexId in polyline3d)
        {
            if (trans.Unwrap().GetObject(vertexId, OpenMode.ForRead) is PolylineVertex3d vertex)
            {
                var rhinoPoint = vertex.Position.ToRhinoPoint3d();
                points.Add(rhinoPoint);

                if (firstPoint)
                {
                    elevation = rhinoPoint.Z;
                    firstPoint = false;
                }
            }
        }

        if (points.Count < 2)
            return null;

        return new Rhino.Geometry.PolylineCurve(points);
    }

    /// <summary>
    /// Converts an AutoCAD Polyline to a Rhino curve.
    /// </summary>
    private static RhinoCurve? ConvertPolylineToRhinoCurve(Polyline pline, out double elevation)
    {
        var points = new List<Rhino.Geometry.Point3d>();
        elevation = 0.0;

        for (var i = 0; i < pline.NumberOfVertices; i++)
        {
            var pt = pline.GetPoint3dAt(i);
            var rhinoPoint = pt.ToRhinoPoint3d();
            points.Add(rhinoPoint);

            if (i == 0)
            {
                elevation = rhinoPoint.Z;
            }
        }

        if (points.Count < 2)
            return null;

        return new Rhino.Geometry.PolylineCurve(points);
    }
}
