namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of a Civil 3D Assembly.
/// </summary>
/// <remarks>
/// This enumeration mirrors <c>Autodesk.Civil.DatabaseServices.AssemblyType</c>
/// from the Civil 3D API.
/// </remarks>
public enum CivilAssemblyType
{
    /// <summary>
    /// An undivided road with a crowned cross section (higher in the center).
    /// </summary>
    UndividedCrownedRoad = 1,

    /// <summary>
    /// An undivided road with a planar (flat) cross section.
    /// </summary>
    UndividedPlanarRoad = 2,

    /// <summary>
    /// A divided road with a crowned cross section on each side.
    /// </summary>
    DividedCrownedRoad = 3,

    /// <summary>
    /// A divided road with a planar (flat) cross section.
    /// </summary>
    DividedPlanarRoad = 4,

    /// <summary>
    /// An assembly type that does not fit standard road categories.
    /// </summary>
    Other = 5,

    /// <summary>
    /// A railway assembly type used for rail track design.
    /// </summary>
    Railway = 6
}
