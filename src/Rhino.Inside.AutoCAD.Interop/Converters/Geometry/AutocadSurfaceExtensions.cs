using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CadCone = Autodesk.AutoCAD.Geometry.Cone;
using CadCylinder = Autodesk.AutoCAD.Geometry.Cylinder;
using CadExtents3d = Autodesk.AutoCAD.DatabaseServices.Extents3d;
using CadFace = Autodesk.AutoCAD.BoundaryRepresentation.Face;
using CadGeometryNurbsSurface = Autodesk.AutoCAD.Geometry.NurbSurface;
using CadGeometrySurface = Autodesk.AutoCAD.Geometry.Surface;
using CadHatch = Autodesk.AutoCAD.DatabaseServices.Hatch;
using CadNurbsSurface = Autodesk.AutoCAD.DatabaseServices.NurbSurface;
using CadSolid3d = Autodesk.AutoCAD.DatabaseServices.Solid3d;
using CadSphere = Autodesk.AutoCAD.Geometry.Sphere;
using CadSurface = Autodesk.AutoCAD.DatabaseServices.Surface;
using CadTorus = Autodesk.AutoCAD.Geometry.Torus;
using RhinoBoundingBox = Rhino.Geometry.BoundingBox;
using RhinoBrep = Rhino.Geometry.Brep;
using RhinoCircle = Rhino.Geometry.Circle;
using RhinoControlPoint = Rhino.Geometry.ControlPoint;
using RhinoHatch = Rhino.Geometry.Hatch;
using RhinoNurbsSurface = Rhino.Geometry.NurbsSurface;
using RhinoPlane = Rhino.Geometry.Plane;
using RhinoPolyCurve = Rhino.Geometry.PolyCurve;
using RhinoSurface = Rhino.Geometry.Surface;
using RhinoVector3d = Rhino.Geometry.Vector3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting AutoCAD surface types to Rhino surface types.
/// </summary>
public static class AutocadSurfaceExtensions
{
    /// <summary>
    /// Converts an AutoCAD Extents3d to a Rhino BoundingBox, applying unit conversion.
    /// </summary>
    /// <param name="extents">The AutoCAD extents to convert.</param>
    /// <returns>A Rhino BoundingBox with coordinates scaled to Rhino units.</returns>
    public static RhinoBoundingBox ToRhinoBoundingBox(this CadExtents3d extents)
    {
        var min = extents.MinPoint.ToRhinoPoint3d();
        var max = extents.MaxPoint.ToRhinoPoint3d();
        return new RhinoBoundingBox(min, max);
    }

    /// <summary>
    /// Converts an AutoCAD NurbSurface to a Rhino NurbsSurface, applying unit conversion.
    /// </summary>
    /// <param name="cadNurbsSurface">The AutoCAD NurbSurface to convert.</param>
    /// <returns>A Rhino NurbsSurface with control points scaled to Rhino units.</returns>
    public static RhinoNurbsSurface ToRhinoNurbsSurface(this CadNurbsSurface cadNurbsSurface)
    {
        var dimension = 3;
        var degreeU = cadNurbsSurface.DegreeInU + 1;
        var degreeV = cadNurbsSurface.DegreeInV + 1;
        var isRational = cadNurbsSurface.IsRational;
        var controlPointsU = cadNurbsSurface.NumberOfControlPointsInU;
        var controlPointsV = cadNurbsSurface.NumberOfControlPointsInV;

        var rhinoSurface = RhinoNurbsSurface.Create(dimension, isRational, degreeU, degreeV, controlPointsU, controlPointsV);

        // Correct Knots from AutoCAD Nurbs Specification
        for (var index = 1; index < cadNurbsSurface.UKnots.Count - 1; index++)
        {
            var uKnot = cadNurbsSurface.UKnots[index];
            rhinoSurface.KnotsU[index - 1] = uKnot;
        }

        for (var index = 1; index < cadNurbsSurface.VKnots.Count - 1; index++)
        {
            var vKnot = cadNurbsSurface.VKnots[index];
            rhinoSurface.KnotsV[index - 1] = vKnot;
        }

        for (var u = 0; u < cadNurbsSurface.NumberOfControlPointsInU; u++)
        {
            for (var v = 0; v < cadNurbsSurface.NumberOfControlPointsInV; v++)
            {
                var controlPoint = cadNurbsSurface.GetControlPointAt(u, v);
                var convertedPoint = controlPoint.ToRhinoPoint3d();
                var weight = cadNurbsSurface.GetWeight(u, v);

                var rhinoControlPoint = new RhinoControlPoint(convertedPoint, weight);
                rhinoSurface.Points.SetControlPoint(u, v, rhinoControlPoint);
            }
        }

        return rhinoSurface;
    }

    /// <summary>
    /// Converts an AutoCAD Hatch to a Rhino Hatch, applying unit conversion.
    /// </summary>
    /// <param name="cadHatch">The AutoCAD Hatch to convert.</param>
    /// <returns>A Rhino Hatch with geometry scaled to Rhino units.</returns>
    public static RhinoHatch ToRhinoHatch(this CadHatch cadHatch)
    {
        var cadPlane = cadHatch.GetPlane();

        var scale = UnitConverter.ToRhinoLength(cadHatch.PatternScale);

        var rotation = cadHatch.PatternAngle;

        // TODO: Support Hatch patterns
        var patternIndex = 1;

        var hatchPlane = cadPlane.ToRhinoPlane();

        var rhinoLoops = new List<RhinoPolyCurve>();
        var externalType = HatchLoopTypes.External;
        var outermostType = HatchLoopTypes.Outermost;

        for (var i = 0; i < cadHatch.NumberOfLoops; i++)
        {
            var hatchLoop = cadHatch.GetLoopAt(i);

            var loopType = hatchLoop.LoopType;

            if ((loopType & externalType) != externalType &&
                (loopType & outermostType) != outermostType) continue;

            var loop = hatchLoop.ToRhinoPolyCurve();

            rhinoLoops.Add(loop);
        }

        return RhinoHatch.Create(hatchPlane, rhinoLoops.FirstOrDefault(), rhinoLoops.Skip(1),
              patternIndex, rotation, scale);
    }

    /// <summary>
    /// Converts an AutoCAD HatchLoop to a Rhino PolyCurve, applying unit conversion.
    /// </summary>
    /// <param name="cadHatchLoop">The AutoCAD HatchLoop to convert.</param>
    /// <returns>A Rhino PolyCurve representing the hatch loop boundary.</returns>
    public static RhinoPolyCurve ToRhinoPolyCurve(this HatchLoop cadHatchLoop)
    {
        var loopCurves = cadHatchLoop.Curves;

        var isPolyLine = cadHatchLoop.IsPolyline;

        return isPolyLine
            ? cadHatchLoop.Polyline.ToRhinoPolyCurve()
            : loopCurves.ToRhinoPolyCurve();
    }

    /// <summary>
    /// Converts a Solid3d to an array of Rhino Breps.
    /// </summary>
    /// <param name="solid">The AutoCAD Solid3d to convert.</param>
    /// <returns>An array of Rhino Breps representing the solid.</returns>
    public static RhinoBrep? ToRhinoBrep(this CadSolid3d solid)
    {
        return BrepConverter.Convert(solid).FirstOrDefault();
    }

    /// <summary>
    /// Converts a <see cref="CadFace"/> to a <see cref="RhinoSurface"/>.
    /// </summary>

    public static RhinoSurface ToRhinoSurface(this CadFace face, out bool parametricOrientation, double relativeTolerance = 0.0)
    {
        var surface = face.Surface;

        parametricOrientation = face.IsOrientToSurface;

        return surface.ToRhinoSurface();

    }

    /// <summary>
    /// Converts a <see cref="CadGeometrySurface"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this CadGeometrySurface cadSurface)
    {
        switch (cadSurface)
        {
            case CadGeometryNurbsSurface nurbsSurface:
                return nurbsSurface.ToRhinoSurface();
            case PlanarEntity planarEntity:
                return planarEntity.ToRhinoSurface();
            case ExternalBoundedSurface boundedSurface:
                return boundedSurface.ToRhinoSurface();
            case ExternalSurface surface:
                return surface.ToRhinoSurface();
            case CadCylinder cylinder:
                return ToRhinoSurface(cylinder);
            case CadCone cone:
                return ToRhinoSurface(cone);
            case CadSphere sphere:
                return ToRhinoSurface(sphere);
            case CadTorus torus:
                return ToRhinoSurface(torus);
            default:
                throw new NotSupportedException($"Unsupported surface type: {cadSurface.GetType().FullName}");
        }
    }

    /// <summary>
    /// Converts a <see cref="ExternalSurface"/>
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this ExternalSurface surface)
    {
        return surface.nativeSurface.ToRhinoSurface();
    }

    /// <summary>
    /// Converts an AutoCAD ExternalBoundedSurface to a Rhino Surface (trimmed if boundaries exist).
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this ExternalBoundedSurface boundedSurface)
    {
        var baseSurface = boundedSurface.BaseSurface;

        var baseRhinoSurface = baseSurface.ToRhinoSurface();

        return baseRhinoSurface;
    }

    /// <summary>
    /// Converts a <see cref="PlanarEntity"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this PlanarEntity planeEntity)
    {
        var origin = planeEntity.PointOnPlane.ToRhinoPoint3d();

        var normal = planeEntity.Normal.ToRhinoVector3d();

        var plane = new RhinoPlane(origin, normal);

        var planeSurface = new Rhino.Geometry.PlaneSurface(plane);

        return planeSurface;
    }

    /// <summary>
    /// Converts a <see cref="CadCylinder"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this CadCylinder cylinder)
    {
        var origin = cylinder.Origin.ToRhinoPoint3d();
        var axis = cylinder.AxisOfSymmetry.ToRhinoVector3d();
        var xAxis = cylinder.ReferenceAxis.ToRhinoVector3d();
        var radius = cylinder.Radius;

        var plane = new RhinoPlane(origin, xAxis,
            RhinoVector3d.CrossProduct(axis, xAxis));

        var circle = new RhinoCircle(plane, radius);

        var interval = cylinder.Height.ToRhinoInterval();

        var rhinoCylinder = new Rhino.Geometry.Cylinder(circle, interval.Length);

        return rhinoCylinder.ToNurbsSurface();
    }

    /// <summary>
    /// Converts a <see cref="CadCone"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(CadCone cone)
    {
        var apex = cone.Apex.ToRhinoPoint3d();
        var basePoint3d = cone.BaseCenter.ToRhinoPoint3d();
        var radius = UnitConverter.ToRhinoLength(cone.BaseRadius);
        var interval = cone.Height.ToRhinoInterval();

        var axis = apex - basePoint3d;
        axis.Unitize();

        var adjustedBase = basePoint3d + axis * interval.T0;
        var height = (apex - adjustedBase).Length;

        // radius at adjustedBase via similar triangles
        var originalHeight = (apex - basePoint3d).Length;
        var adjustedRadius = radius * height / originalHeight;

        // Rhino Cone: plane origin is apex, normal points toward base
        var plane = new RhinoPlane(apex, -axis);
        var rhinoCone = new Rhino.Geometry.Cone(plane, height, adjustedRadius);
        return rhinoCone.ToNurbsSurface();
    }

    /// <summary>
    /// Converts a <see cref="CadSphere"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(CadSphere sphere)
    {
        var center = sphere.Center.ToRhinoPoint3d();

        var radius = UnitConverter.ToRhinoLength(sphere.Radius);

        var rhinoCone = new Rhino.Geometry.Sphere(center, radius);

        return rhinoCone.ToNurbsSurface();
    }

    /// <summary>
    /// Converts a <see cref="CadTorus"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(CadTorus torus)
    {
        var center = torus.Center.ToRhinoPoint3d();

        var axis = torus.AxisOfSymmetry.ToRhinoVector3d();

        var xAxis = torus.ReferenceAxis.ToRhinoVector3d();

        var plane = new RhinoPlane(center, xAxis,
            RhinoVector3d.CrossProduct(axis, xAxis));

        var majorRadius = UnitConverter.ToRhinoLength(torus.MajorRadius);
        var minorRadius = UnitConverter.ToRhinoLength(torus.MinorRadius);

        var rhinoCone = new Rhino.Geometry.Torus(plane, majorRadius, minorRadius);

        return rhinoCone.ToNurbsSurface();
    }

    /// <summary>
    /// Converts a <see cref="CadGeometryNurbsSurface"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this CadGeometryNurbsSurface nurbsSurface)
    {
        var dimension = 3;
        var degreeU = nurbsSurface.DegreeInU + 1;
        var degreeV = nurbsSurface.DegreeInV + 1;
        var isRational = nurbsSurface.IsRationalInU && nurbsSurface.IsRationalInV;
        var controlPointsU = nurbsSurface.NumControlPointsInU;
        var controlPointsV = nurbsSurface.NumControlPointsInV;

        var rhinoSurface = RhinoNurbsSurface.Create(dimension, isRational, degreeU, degreeV, controlPointsU, controlPointsV);

        // Correct Knots from AutoCAD Nurbs Specification
        for (var index = 1; index < nurbsSurface.UKnots.Count - 1; index++)
        {
            var uKnot = nurbsSurface.UKnots[index];
            rhinoSurface.KnotsU[index - 1] = uKnot;
        }

        for (var index = 1; index < nurbsSurface.VKnots.Count - 1; index++)
        {
            var vKnot = nurbsSurface.VKnots[index];
            rhinoSurface.KnotsV[index - 1] = vKnot;
        }

        for (var u = 0; u < controlPointsU; u++)
        {
            for (var v = 0; v < controlPointsV; v++)
            {
                var index = u * controlPointsV + v;
                var controlPoint = nurbsSurface.ControlPoints[index];
                var convertedPoint = controlPoint.ToRhinoPoint3d();
                var weight = nurbsSurface.Weights[index];

                var rhinoControlPoint = new RhinoControlPoint(convertedPoint, weight);
                rhinoSurface.Points.SetControlPoint(u, v, rhinoControlPoint);
            }
        }

        return rhinoSurface;
    }

    /// <summary>
    /// Converts a <see cref="CadSurface"/> to a <see cref="RhinoSurface"/>.
    /// </summary>
    public static RhinoSurface ToRhinoSurface(this CadSurface cadSurface)
    {
        switch (cadSurface)
        {
            case CadNurbsSurface nurbsSurface:
                return nurbsSurface.ToRhinoNurbsSurface();
            default:
                throw new NotSupportedException($"Unsupported surface type: {cadSurface.GetType().FullName}");
        }
    }
}
