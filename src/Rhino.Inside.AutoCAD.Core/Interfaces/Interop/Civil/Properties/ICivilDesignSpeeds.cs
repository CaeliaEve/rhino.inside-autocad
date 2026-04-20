namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents design speed information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Contains the base design speed and a collection of speed stations
/// that define speed variations along the alignment.
/// </remarks>
public interface ICivilDesignSpeeds
{
    /// <summary>
    /// Gets the base design speed for the alignment.
    /// </summary>
    double DesignSpeed { get; }

    /// <summary>
    /// Gets the collection of speed stations along the alignment.
    /// </summary>
    IReadOnlyList<ICivilDesignSpeedStation> SpeedStations { get; }

    /// <summary>
    /// Creates a shallow copy of this design speeds information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilDesignSpeeds ShallowClone();
}

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
