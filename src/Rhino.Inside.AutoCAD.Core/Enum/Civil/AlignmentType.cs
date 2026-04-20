namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This enumeration mirrors <c>Autodesk.Civil.DatabaseServices.AlignmentType</c>
/// from the Civil 3D API.
/// </remarks>
public enum AlignmentType
{
    /// <summary>
    /// A centerline alignment representing the primary path or road centerline.
    /// </summary>
    Centerline = 1,

    /// <summary>
    /// An offset alignment that runs parallel to another alignment at a specified distance.
    /// </summary>
    Offset = 2,

    /// <summary>
    /// A curb return alignment used for intersection corners and transitions.
    /// </summary>
    CurbReturn = 3,

    /// <summary>
    /// A utility alignment used for routing utilities such as pipes or conduits.
    /// </summary>
    Utility = 4,

    /// <summary>
    /// A rail alignment used for railway track design.
    /// </summary>
    Rail = 5,
}
