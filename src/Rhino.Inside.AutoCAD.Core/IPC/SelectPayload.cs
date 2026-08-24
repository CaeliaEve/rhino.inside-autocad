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
/// Data transfer object for a selected AutoCAD object.
/// </summary>
public class SelectedObjectDto
{
    public string Handle { get; set; } = string.Empty;
    public string Layer { get; set; } = "0";
    public int ColorRgb { get; set; } = -1;
    public string ObjectType { get; set; } = string.Empty;
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
