using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Converters;
using Rhino.Inside.AutoCAD.Core.IPC;
using Xunit;

namespace Rhino.Inside.AutoCAD.Tests;

public class CadCurveReconstructorTests
{
    public CadCurveReconstructorTests()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var shortName = new System.Reflection.AssemblyName(e.Name).Name;
            if (shortName == "RhinoCommon")
            {
                var p = @"D:\software\Rhino8\System\RhinoCommon.dll";
                if (System.IO.File.Exists(p)) return System.Reflection.Assembly.LoadFrom(p);
            }
            return null;
        };
    }

    [Fact]
    public void LineDto_ToRhinoCurve_ShouldCreateValidLine()
    {
        var dto = new CadCurveDto
        {
            CurveType = "Line",
            Points = new List<double[]>
            {
                new double[] { 0, 0, 0 },
                new double[] { 100, 50, 0 }
            }
        };

        var crv = CadCurveReconstructor.ToRhinoCurve(dto);
        Assert.NotNull(crv);
        Assert.True(crv.IsValid);
        Assert.Equal(0, crv.PointAtStart.X);
        Assert.Equal(100, crv.PointAtEnd.X);
    }

    [Fact]
    public void ArcDto_ToRhinoCurve_ShouldCreateValidArc()
    {
        var dto = new CadCurveDto
        {
            CurveType = "Arc",
            Center = new double[] { 10, 20, 0 },
            Radius = 50,
            StartAngle = 0,
            EndAngle = Math.PI,
            Normal = new double[] { 0, 0, 1 }
        };

        var crv = CadCurveReconstructor.ToRhinoCurve(dto);
        Assert.NotNull(crv);
        Assert.True(crv.IsValid);
    }

    [Fact]
    public void PolylineDto_ToRhinoCurve_ShouldCreateValidPolyline()
    {
        var dto = new CadCurveDto
        {
            CurveType = "Polyline",
            IsClosed = true,
            Normal = new double[] { 0, 0, 1 },
            Points = new List<double[]>
            {
                new double[] { 0, 0, 0 },
                new double[] { 100, 0, 0 },
                new double[] { 100, 100, 0 },
                new double[] { 0, 100, 0 }
            },
            Bulges = new List<double> { 0, 0, 0, 0 }
        };

        var crv = CadCurveReconstructor.ToRhinoCurve(dto);
        Assert.NotNull(crv);
        Assert.True(crv.IsValid);
    }
}
