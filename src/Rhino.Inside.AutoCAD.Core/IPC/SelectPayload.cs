using System;
using System.Collections.Generic;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Request payload for cross-process AutoCAD object selection.
/// </summary>
public class SelectRequestPayload
{
    public string PromptMessage { get; set; } = "Select AutoCAD Object";
    public bool SingleOnly { get; set; } = true;
    public string TargetType { get; set; } = "Curve"; // "Curve", "Point", "Mesh", "Solid", "All"
}

/// <summary>
/// Mathematical representation of an AutoCAD curve without requiring rhcommon_c native dependencies in AutoCAD.
/// </summary>
public class CadCurveDto
{
    public string CurveType { get; set; } = "Line"; // "Line", "Arc", "Circle", "Polyline", "Polyline3d", "Spline", "Ellipse"
    public List<double[]> Points { get; set; } = new();
    public List<double> Bulges { get; set; } = new();
    public double[] Center { get; set; } = Array.Empty<double>();
    public double Radius { get; set; }
    public double StartAngle { get; set; }
    public double EndAngle { get; set; }
    public double[] Normal { get; set; } = new double[] { 0, 0, 1 };
    public int Degree { get; set; } = 3;
    public List<double> Knots { get; set; } = new();
    public List<double> Weights { get; set; } = new();
    public bool IsClosed { get; set; }
    public bool IsRational { get; set; }
    public bool IsPeriodic { get; set; }
    public double[] MajorAxis { get; set; } = Array.Empty<double>();
    public double RadiusRatio { get; set; } = 1.0;
}

/// <summary>
/// Data transfer object for a selected AutoCAD object.
/// </summary>
public class SelectedObjectDto
{
    public string Handle { get; set; } = string.Empty;
    public string Layer { get; set; } = "0";
    public int ColorRgb { get; set; } = -1;
    public string ObjectType { get; set; } = string.Empty;
    public CadCurveDto? CurveData { get; set; }
    public byte[] Geometry3dmBytes { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Response payload containing selected AutoCAD objects.
/// </summary>
public class SelectResponsePayload
{
    public bool Success { get; set; }
    public List<SelectedObjectDto> Objects { get; set; } = new();
}
