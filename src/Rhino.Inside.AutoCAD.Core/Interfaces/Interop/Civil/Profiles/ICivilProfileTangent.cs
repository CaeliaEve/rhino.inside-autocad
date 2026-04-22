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
}
