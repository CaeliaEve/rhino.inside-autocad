using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a tangent (straight line) entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile tangents are grade-based straight segments between vertical curves.
/// </remarks>
public interface ICivilProfileTangent : ICivilProfileEntity
{
    /// <summary>
    /// Gets the grade (slope) of this tangent as a percentage.
    /// </summary>
    /// <remarks>
    /// Positive values indicate uphill direction, negative values indicate downhill.
    /// </remarks>
    double Grade { get; }

    /// <summary>
    /// Gets the geometry of this tangent as a Rhino line.
    /// </summary>
    /// <remarks>
    /// The line is in 2D station-elevation space where X = Station and Y = Elevation.
    /// </remarks>
    Line Line { get; }
}
