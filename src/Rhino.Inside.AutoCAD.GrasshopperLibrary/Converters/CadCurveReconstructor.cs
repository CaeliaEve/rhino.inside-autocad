using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.IPC;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary.Converters;

/// <summary>
/// Reconstructs native Rhino geometry curves from pure mathematical CadCurveDto payloads.
/// Executes entirely in Rhino process space with native rhcommon_c support.
/// </summary>
public static class CadCurveReconstructor
{
    /// <summary>
    /// Converts a <see cref="CadCurveDto"/> to a native <see cref="Curve"/>.
    /// </summary>
    public static Curve? ToRhinoCurve(CadCurveDto dto)
    {
        if (dto == null) return null;

        try
        {
            switch (dto.CurveType)
            {
                case "Line":
                    if (dto.Points.Count >= 2)
                    {
                        var p0 = new Point3d(dto.Points[0][0], dto.Points[0][1], dto.Points[0][2]);
                        var p1 = new Point3d(dto.Points[1][0], dto.Points[1][1], dto.Points[1][2]);
                        return new LineCurve(p0, p1);
                    }
                    break;

                case "Circle":
                    if (dto.Center.Length >= 3 && dto.Radius > 0)
                    {
                        var center = new Point3d(dto.Center[0], dto.Center[1], dto.Center[2]);
                        var normal = dto.Normal.Length >= 3 ? new Vector3d(dto.Normal[0], dto.Normal[1], dto.Normal[2]) : Vector3d.ZAxis;
                        if (normal.IsZero) normal = Vector3d.ZAxis;
                        var plane = new Plane(center, normal);
                        return new ArcCurve(new Circle(plane, dto.Radius));
                    }
                    break;

                case "Arc":
                    if (dto.Center.Length >= 3 && dto.Radius > 0)
                    {
                        var center = new Point3d(dto.Center[0], dto.Center[1], dto.Center[2]);
                        var normal = dto.Normal.Length >= 3 ? new Vector3d(dto.Normal[0], dto.Normal[1], dto.Normal[2]) : Vector3d.ZAxis;
                        if (normal.IsZero) normal = Vector3d.ZAxis;
                        var plane = new Plane(center, normal);
                        var circle = new Circle(plane, dto.Radius);
                        double start = dto.StartAngle;
                        double end = dto.EndAngle;
                        if (end < start) end += 2.0 * Math.PI;
                        var interval = new Interval(start, end);
                        return new ArcCurve(new Arc(circle, interval));
                    }
                    break;

                case "Polyline":
                    if (dto.Points.Count >= 2)
                    {
                        var polyCurve = new PolyCurve();
                        int segmentCount = dto.IsClosed ? dto.Points.Count : dto.Points.Count - 1;

                        for (int i = 0; i < segmentCount; i++)
                        {
                            int nextIdx = (i + 1) % dto.Points.Count;
                            var p0 = new Point3d(dto.Points[i][0], dto.Points[i][1], dto.Points[i][2]);
                            var p1 = new Point3d(dto.Points[nextIdx][0], dto.Points[nextIdx][1], dto.Points[nextIdx][2]);

                            double bulge = (dto.Bulges != null && dto.Bulges.Count > i) ? dto.Bulges[i] : 0.0;

                            if (Math.Abs(bulge) < 1e-7)
                            {
                                polyCurve.Append(new LineCurve(p0, p1));
                            }
                            else
                            {
                                double theta = 4.0 * Math.Atan(bulge);
                                var chord = p1 - p0;
                                double chordLen = chord.Length;

                                if (chordLen > 1e-7)
                                {
                                    var normVec = dto.Normal.Length >= 3 ? new Vector3d(dto.Normal[0], dto.Normal[1], dto.Normal[2]) : Vector3d.ZAxis;
                                    var sideVec = Vector3d.CrossProduct(normVec, chord);
                                    sideVec.Unitize();

                                    double sagitta = (chordLen / 2.0) * bulge;
                                    var mid = (p0 + p1) * 0.5;
                                    var arcPt = mid + sideVec * sagitta;

                                    var arc = new Arc(p0, arcPt, p1);
                                    if (arc.IsValid)
                                    {
                                        polyCurve.Append(new ArcCurve(arc));
                                    }
                                    else
                                    {
                                        polyCurve.Append(new LineCurve(p0, p1));
                                    }
                                }
                            }
                        }

                        return polyCurve;
                    }
                    break;

                case "Polyline3d":
                    if (dto.Points.Count >= 2)
                    {
                        var pts = dto.Points.Select(p => new Point3d(p[0], p[1], p[2])).ToList();
                        if (dto.IsClosed && pts.Count > 0 && pts[0].DistanceTo(pts[pts.Count - 1]) > 1e-7)
                        {
                            pts.Add(pts[0]);
                        }
                        var pl = new Polyline(pts);
                        return pl.ToPolylineCurve();
                    }
                    break;

                case "Spline":
                    if (dto.Points.Count >= 2)
                    {
                        int order = dto.Degree + 1;
                        int cvCount = dto.Points.Count;
                        var nurbs = new NurbsCurve(3, dto.IsRational, order, cvCount);
                        if (nurbs != null)
                        {
                            for (int i = 0; i < cvCount; i++)
                            {
                                var pt = new Point3d(dto.Points[i][0], dto.Points[i][1], dto.Points[i][2]);
                                double w = (dto.Weights != null && dto.Weights.Count > i) ? dto.Weights[i] : 1.0;
                                nurbs.Points.SetPoint(i, pt.X, pt.Y, pt.Z, w);
                            }

                            if (dto.Knots != null)
                            {
                                for (int i = 0; i < dto.Knots.Count && i < nurbs.Knots.Count; i++)
                                {
                                    nurbs.Knots[i] = dto.Knots[i];
                                }
                            }

                            return nurbs;
                        }
                    }
                    break;

                case "Ellipse":
                    if (dto.Center.Length >= 3 && dto.MajorAxis.Length >= 3)
                    {
                        var center = new Point3d(dto.Center[0], dto.Center[1], dto.Center[2]);
                        var normal = dto.Normal.Length >= 3 ? new Vector3d(dto.Normal[0], dto.Normal[1], dto.Normal[2]) : Vector3d.ZAxis;
                        var major = new Vector3d(dto.MajorAxis[0], dto.MajorAxis[1], dto.MajorAxis[2]);
                        var minor = Vector3d.CrossProduct(normal, major);
                        minor.Unitize();
                        double rMinor = major.Length * dto.RadiusRatio;
                        minor *= rMinor;

                        var plane = new Plane(center, major, minor);
                        var ellipse = new Ellipse(plane, major.Length, rMinor);
                        return ellipse.ToNurbsCurve();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CadCurveReconstructor] Reconstruction error: {ex.Message}");
        }

        return null;
    }
}
