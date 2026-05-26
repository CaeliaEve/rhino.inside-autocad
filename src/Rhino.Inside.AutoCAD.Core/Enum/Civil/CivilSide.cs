namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the side of a Civil 3D Subassembly relative to the assembly baseline.
/// </summary>
/// <remarks>
/// This enumeration mirrors <c>Autodesk.Civil.DatabaseServices.SubassemblySide</c>
/// from the Civil 3D API.
/// </remarks>
public enum CivilSide
{
    /// <summary>
    /// No specific side assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// The left side of the assembly baseline.
    /// </summary>
    Left = 1,

    /// <summary>
    /// The right side of the assembly baseline.
    /// </summary>
    Right = 2
}
