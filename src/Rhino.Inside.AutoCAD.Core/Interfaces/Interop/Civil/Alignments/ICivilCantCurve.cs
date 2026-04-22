namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a CANT curve along an alignment.
/// </summary>
public interface ICivilCantCurve
{
    /// <summary>
    /// Gets the starting station of this CANT curve.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this CANT curve.
    /// </summary>
    double EndStation { get; }
}