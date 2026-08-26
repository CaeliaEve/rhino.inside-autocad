using System;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Serializable payload for cross-process Bake requests between Rhino 8 and AutoCAD.
/// </summary>
public class BakePayload
{
    public string TargetLayer { get; set; } = "0";
    public int ColorRgb { get; set; } = -1; // -1 means ByLayer
    public string Linetype { get; set; } = "ByLayer";
    public bool ReplaceExisting { get; set; } = false;
    public string? BlockName { get; set; }
    public System.Collections.Generic.List<CadCurveDto> Curves { get; set; } = new();
    public byte[] Geometry3dmBytes { get; set; } = Array.Empty<byte>();
}
