using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps design speed information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilDesignSpeeds : ICivilDesignSpeeds
{

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

        this.SpeedStations = Array.Empty<ICivilDesignSpeedStation>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilDesignSpeeds"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract design speeds from.</param>
    public CivilDesignSpeeds(Alignment alignment)
    {
        try
        {

            var stations = new List<ICivilDesignSpeedStation>();
            var speedTable = alignment.DesignSpeeds;

            if (speedTable != null)
            {
                foreach (var speed in speedTable)
                {
                    stations.Add(new CivilDesignSpeedStation(speed));
                }
            }

            this.SpeedStations = stations;
        }
        catch
        {
            this.SpeedStations = Array.Empty<ICivilDesignSpeedStation>();
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
        return $"Design Speed ({this.SpeedStations.Count} speed stations)";
    }
}
