using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using CivilSurfaceBreaklineType = Autodesk.Civil.SurfaceBreaklineType;
using RhinoCurve = Rhino.Geometry.Curve;
using SurfaceBreaklineType = Rhino.Inside.AutoCAD.Core.SurfaceBreaklineType;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for extracting breakline data from Civil 3D TIN Surfaces.
/// </summary>
public static class CivilSurfaceBreaklineExtensions
{
    /// <summary>
    /// Extracts all breaklines from a TIN surface.
    /// </summary>
    /// <param name="surfaceRaw">The TIN surface to extract breaklines from.</param>
    /// <param name="transaction">The current AutoCAD transaction.</param>
    /// <returns>A list of breakline wrappers containing the extracted breakline data.</returns>
    public static IReadOnlyList<CivilSurfaceBreakline> GetBreaklines(
        this TinSurface surfaceRaw,
        IAutocadTransactionManager transaction)
    {
        var breaklines = new List<CivilSurfaceBreakline>();

        var surface = transaction.Unwrap()
            .GetObject(surfaceRaw.Id, OpenMode.ForRead) as TinSurface;

        if (surface == null)
            return breaklines;

        // Process breakline definitions from BreaklinesDefinition
        var breaklinesDefinition = surface.BreaklinesDefinition;

        for (var i = 0; i < breaklinesDefinition.Count; i++)
        {
            var breaklineOp = breaklinesDefinition[i];

            // Get breakline type and name from the operation
            var breaklineType = ConvertBreaklineType(breaklineOp.BreaklineType);
            var opName = breaklineOp.Description ?? $"Breakline_{i}";

            // SurfaceOperationAddBreakline contains breakline data
            // Each breakline operation can contain multiple breaklines
            for (var j = 0; j < breaklineOp.Count; j++)
            {
                var breaklineData = breaklineOp[j];
                var curve = ExtractBreaklineCurve(breaklineData);

                if (curve != null)
                {
                    var name = breaklineOp.Count > 1 ? $"{opName}_{j}" : opName;
                    breaklines.Add(new CivilSurfaceBreakline(breaklineType, curve, name));
                }
            }
        }

        return breaklines;
    }

    /// <summary>
    /// Extracts the curve geometry from a SurfaceBreakline.
    /// </summary>
    private static RhinoCurve? ExtractBreaklineCurve(SurfaceBreakline surfaceBreakline)
    {
        try
        {
            var points = new List<Rhino.Geometry.Point3d>();

            var vertices = surfaceBreakline.Vertices;

            foreach (CadPoint3d vertex in vertices)
            {
                points.Add(vertex.ToRhinoPoint3d());
            }

            if (points.Count < 2)
                return null;

            return new Rhino.Geometry.PolylineCurve(points);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts the Civil 3D SurfaceBreaklineType enum to our SurfaceBreaklineType enum.
    /// </summary>
    private static SurfaceBreaklineType ConvertBreaklineType(CivilSurfaceBreaklineType breaklineType)
    {
        return breaklineType switch
        {
            CivilSurfaceBreaklineType.Standard => SurfaceBreaklineType.Standard,
            CivilSurfaceBreaklineType.Wall => SurfaceBreaklineType.Wall,
            CivilSurfaceBreaklineType.NonDestructive => SurfaceBreaklineType.NonDestructive,
            _ => SurfaceBreaklineType.Standard
        };
    }
}
