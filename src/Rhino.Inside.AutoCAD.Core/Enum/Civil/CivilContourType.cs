namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of contour line on a Civil 3D TIN Surface.
/// </summary>
public enum CivilContourType
{
    /// <summary>
    /// Major contour lines (typically thicker, at larger elevation intervals).
    /// </summary>
    Major = 1,

    /// <summary>
    /// Minor contour lines (typically thinner, at smaller elevation intervals).
    /// </summary>
    Minor = 2
}
