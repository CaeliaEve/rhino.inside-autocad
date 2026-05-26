using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a feature line extracted from a Civil 3D Corridor baseline.
/// </summary>
/// <remarks>
/// Feature lines are coded geometry extracted from corridor cross-sections
/// that follow the path of the corridor with specific point codes.
/// </remarks>
public interface ICivilCorridorFeatureLine
{
    /// <summary>
    /// Gets the point code associated with this feature line.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the Rhino curve representing this feature line geometry.
    /// </summary>
    Curve Curve { get; }
}
