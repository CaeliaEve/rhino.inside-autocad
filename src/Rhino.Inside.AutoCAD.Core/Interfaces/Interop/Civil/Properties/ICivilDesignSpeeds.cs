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
    /// Gets the collection of speed stations along the alignment.
    /// </summary>
    IReadOnlyList<ICivilDesignSpeedStation> SpeedStations { get; }

    /// <summary>
    /// Creates a shallow copy of this design speeds information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilDesignSpeeds ShallowClone();
}