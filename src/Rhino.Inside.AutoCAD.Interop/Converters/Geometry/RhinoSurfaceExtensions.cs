using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using CadHatch = Autodesk.AutoCAD.DatabaseServices.Hatch;
using CadNurbsSurface = Autodesk.AutoCAD.DatabaseServices.NurbSurface;
using CadSolid3d = Autodesk.AutoCAD.DatabaseServices.Solid3d;
using CadSurface = Autodesk.AutoCAD.DatabaseServices.Surface;
using Region = Autodesk.AutoCAD.DatabaseServices.Region;
using RhinoBrep = Rhino.Geometry.Brep;
using RhinoHatch = Rhino.Geometry.Hatch;
using RhinoNurbsSurface = Rhino.Geometry.NurbsSurface;
using RhinoSurface = Rhino.Geometry.Surface;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Rhino surface types to AutoCAD surface types.
/// </summary>
public static class RhinoSurfaceExtensions
{
    /// <summary>
    /// Converts a Rhino NurbsSurface to an AutoCAD NurbSurface, applying unit conversion.
    /// </summary>
    /// <param name="surface">The Rhino NurbsSurface to convert.</param>
    /// <returns>An AutoCAD NurbSurface with control points scaled to AutoCAD units.</returns>
    public static CadNurbsSurface ToAutocadNurbSurface(this RhinoNurbsSurface surface)
    {
        var degreeU = surface.OrderU - 1;
        var degreeV = surface.OrderV - 1;
        var isRational = surface.IsRational;
        var controlPointsU = surface.Points.CountU;
        var controlPointsV = surface.Points.CountV;

        var uKnots = new KnotCollection();
        uKnots.Add(surface.KnotsU.First());
        foreach (var uKnot in surface.KnotsU)
        {
            uKnots.Add(uKnot);
        }
        uKnots.Add(surface.KnotsU.Last());

        var vKnots = new KnotCollection();
        vKnots.Add(surface.KnotsV.First());
        foreach (var vKnot in surface.KnotsV)
        {
            vKnots.Add(vKnot);
        }
        vKnots.Add(surface.KnotsV.Last());

        var controlPoints = new Point3dCollection();
        var weights = new DoubleCollection();

        for (var u = 0; u < controlPointsU; u++)
        {
            for (var v = 0; v < controlPointsV; v++)
            {
                var controlPoint = surface.Points.GetControlPoint(u, v);
                var convertedPoint = controlPoint.Location.ToAutocadPoint3d();
                var weight = controlPoint.Weight;

                if (isRational)
                    weights.Add(weight);

                controlPoints.Add(convertedPoint);
            }
        }

        var cadSurface = new CadNurbsSurface();
        cadSurface.Set(degreeU, degreeV, isRational, controlPointsU, controlPointsV, controlPoints, weights, uKnots, vKnots);

        return cadSurface;
    }

    /// <summary>
    /// Converts a Rhino Surface to an AutoCAD NurbSurface, applying unit conversion.
    /// </summary>
    /// <param name="surface">The Rhino Surface to convert.</param>
    /// <returns>An AutoCAD NurbSurface with control points scaled to AutoCAD units.</returns>
    public static CadNurbsSurface ToAutocadNurbSurface(this RhinoSurface surface)
    {
        var nurbs = surface.ToNurbsSurface();
        return nurbs.ToAutocadNurbSurface();
    }

    /// <summary>
    /// Converts a Rhino Brep to an array of AutoCAD NurbSurfaces, applying unit conversion.
    /// Each face of the Brep is converted to a separate NurbSurface.
    /// </summary>
    /// <param name="brep">The Rhino Brep to convert.</param>
    /// <returns>An array of AutoCAD NurbSurfaces representing the Brep faces.</returns>
    public static CadNurbsSurface[] ToAutocadNurbSurfaces(this Brep brep)
    {
        var cadFaces = new List<CadNurbsSurface>();

        foreach (var face in brep.Faces)
        {
            var trimmedSurface = face.DuplicateFace(false);
            var singleFace = trimmedSurface.Faces[0];
            var nurbs = singleFace.ToNurbsSurface();
            var cadSurface = nurbs.ToAutocadNurbSurface();

            cadFaces.Add(cadSurface);
        }

        return cadFaces.ToArray();
    }

    /// <summary>
    /// Converts a Rhino Hatch to an AutoCAD Hatch, applying unit conversion.
    /// </summary>
    /// <param name="rhinoHatch">The Rhino Hatch to convert.</param>
    /// <param name="autocadTransactionManager">The transaction manager for database operations.</param>
    /// <returns>An AutoCAD Hatch.</returns>
    public static CadHatch ToAutocadHatch(this RhinoHatch rhinoHatch, IAutocadTransactionManager autocadTransactionManager)
    {
        var scale = UnitConverter.ToAutoCadLength(rhinoHatch.PatternScale);

        var origin = rhinoHatch.BasePoint.ToAutocadPoint2d();

        var cadHatch = new CadHatch()
        {
            PatternScale = scale,
            Origin = origin,
        };

        cadHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");

        var outerCurves = rhinoHatch.Get3dCurves(true);

        var outerCurve = new PolyCurve();
        foreach (var curve in outerCurves)
        {
            outerCurve.Append(curve);
        }

        var transaction = autocadTransactionManager.Unwrap();

        var modelSpace = autocadTransactionManager.GetModelSpace(true).UnwrapObject() as BlockTableRecord;

        var objectIds = new ObjectIdCollection();

        var outerLoop = outerCurve.ToAutocadCurves();

        foreach (var curve in outerLoop)
        {
            modelSpace!.AppendEntity(curve);

            transaction.AddNewlyCreatedDBObject(curve, true);

            objectIds.Add(curve.ObjectId);
        }

        foreach (var innerCurve in rhinoHatch.Get3dCurves(false))
        {
            var innerLoop = innerCurve.ToAutocadCurves();

            foreach (var curve in innerLoop)
            {
                modelSpace!.AppendEntity(curve);

                transaction.AddNewlyCreatedDBObject(curve, true);

                objectIds.Add(curve.ObjectId);
            }
        }

        cadHatch.AppendLoop(HatchLoopTypes.External, objectIds);

        cadHatch.EvaluateHatch(true);

        foreach (ObjectId objectId in objectIds)
        {
            var dbObject = transaction.GetObject(objectId, OpenMode.ForWrite);
            dbObject.Erase(true);
        }

        return cadHatch;
    }

    /// <summary>
    /// Converts a Rhino Extrusion to an array of AutoCAD Solid3ds, applying unit conversion.
    /// </summary>
    /// <param name="extrusion">The Rhino Extrusion to convert.</param>
    /// <param name="autocadTransactionManager">The transaction manager for database operations.</param>
    /// <returns>An array of AutoCAD Solid3d objects.</returns>
    public static CadSolid3d[] ToAutocadSolid3ds(this Extrusion extrusion, IAutocadTransactionManager autocadTransactionManager)
    {
        var solids = new List<CadSolid3d>();

        try
        {
            using var curves = new DBObjectCollection();

            var profileCount = extrusion.ProfileCount;

            for (var i = 0; i < profileCount; i++)
            {
                var profile = extrusion.Profile3d(i, 0);

                if (profile == null)
                    continue;

                var cadCurves = profile.ToAutocadCurves();

                foreach (var cadCurve in cadCurves)
                {
                    curves.Add(cadCurve);
                }
            }

            var regions = Region.CreateFromCurves(curves);

            foreach (Region region in regions)
            {
                var solid = new CadSolid3d();

                var extrusionLine = extrusion.PathLineCurve();

                var extrusionVector = extrusionLine.PointAtEnd - extrusionLine.PointAtStart;

                var cadExtrusionVector = extrusionVector.ToAutocadVector3d();

                var magnitude = extrusionVector.Length;

                var cadMagnitude = UnitConverter.ToAutoCadLength(magnitude);

                var directionVector = cadExtrusionVector.MultiplyBy(cadMagnitude);

                var sweepOptions = new SweepOptions();

                solid.CreateExtrudedSolid(region, directionVector, sweepOptions);

                solids.Add(solid);

                region.Dispose();
            }
        }
        catch
        {
            // Swallow exceptions during conversion
        }

        return solids.ToArray();
    }

    /// <summary>
    /// Converts a Rhino Brep to an AutoCAD BrepProxy representation.
    /// The proxy stores the Brep faces as AutoCAD NurbSurfaces.
    /// </summary>
    /// <param name="rhinoBrep">The Rhino Brep to convert.</param>
    /// <returns>An AutocadBrepProxy containing NurbSurface representations of the Brep faces.</returns>
    public static AutocadBrepProxy? ToAutocadBrepProxy(this RhinoBrep rhinoBrep)
    {
        var faces = new List<CadSurface>();

        foreach (var brepFace in rhinoBrep.Faces)
        {
            var trimmedFace = brepFace.DuplicateFace(false);

            var nurbsSurface = trimmedFace.Faces[0].ToNurbsSurface();

            var cadNurbsSurface = nurbsSurface.ToAutocadNurbSurface();

            faces.Add(cadNurbsSurface);
        }

        return new AutocadBrepProxy(faces);
    }
}
