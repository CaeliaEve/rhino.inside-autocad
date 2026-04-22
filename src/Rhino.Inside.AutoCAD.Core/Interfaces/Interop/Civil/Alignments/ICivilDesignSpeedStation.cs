namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a design speed at a specific station along an alignment.
/// </summary>
public interface ICivilDesignSpeedStation
{
    /// <summary>
    /// Gets the station value where this speed applies.
    /// </summary>
    double Station { get; }

    /// <summary>
    /// Gets the design speed value at this station.
    /// </summary>
    double Speed { get; }
}
