namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a CANT critical station along an alignment.
/// </summary>
public interface ICivilCantCriticalStation
{
    /// <summary>
    /// Gets the station value of this critical station.
    /// </summary>
    double Station { get; }

    /// <summary>
    /// Gets the type of this critical station.
    /// </summary>
    string StationType { get; }
}