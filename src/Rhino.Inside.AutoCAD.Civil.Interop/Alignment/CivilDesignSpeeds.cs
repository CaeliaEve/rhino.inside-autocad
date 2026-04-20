using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps design speed information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilDesignSpeeds : ICivilDesignSpeeds
{
    /// <inheritdoc />
    public double DesignSpeed { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilDesignSpeedStation> SpeedStations { get; }

    /// <summary>
    /// Gets an empty design speeds instance with no data.
    /// </summary>
    public static CivilDesignSpeeds Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilDesignSpeeds"/>.
    /// </summary>
    private CivilDesignSpeeds()
    {
        DesignSpeed = 0;
        SpeedStations = Array.Empty<ICivilDesignSpeedStation>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilDesignSpeeds"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract design speeds from.</param>
    public CivilDesignSpeeds(Alignment alignment)
    {
        try
        {
            DesignSpeed = alignment.DesignSpeed;

            var stations = new List<ICivilDesignSpeedStation>();
            var speedTable = alignment.DesignSpeeds;

            if (speedTable != null)
            {
                foreach (DesignSpeed speed in speedTable)
                {
                    stations.Add(new CivilDesignSpeedStation(speed.Station, speed.Value));
                }
            }

            SpeedStations = stations;
        }
        catch
        {
            DesignSpeed = 0;
            SpeedStations = Array.Empty<ICivilDesignSpeedStation>();
        }
    }

    /// <inheritdoc />
    public ICivilDesignSpeeds ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this design speeds information.
    /// </summary>
    public override string ToString()
    {
        return $"Design Speed: {DesignSpeed:F2} ({SpeedStations.Count} speed stations)";
    }
}

/// <summary>
/// Represents a design speed at a specific station.
/// </summary>
public record CivilDesignSpeedStation : ICivilDesignSpeedStation
{
    /// <inheritdoc />
    public double Station { get; }

    /// <inheritdoc />
    public double Speed { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilDesignSpeedStation"/>.
    /// </summary>
    /// <param name="station">The station value.</param>
    /// <param name="speed">The speed value.</param>
    public CivilDesignSpeedStation(double station, double speed)
    {
        Station = station;
        Speed = speed;
    }

    /// <summary>
    /// Returns a string representation of this speed station.
    /// </summary>
    public override string ToString()
    {
        return $"Station {Station:F2}: {Speed:F2}";
    }
}
