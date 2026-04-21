using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted corridor property information.
/// The data is captured at construction time from a <see cref="Corridor"/>.
/// </remarks>
public record CivilCorridorProperties : ICivilCorridorProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilCorridorProperties"/> by extracting
    /// data from a given <see cref="Corridor"/>.
    /// </summary>
    public static CivilCorridorProperties CreateFromCorridor(Corridor corridor)
    {
        // Calculate corridor extents from baselines
        var minStation = double.MaxValue;
        var maxStation = double.MinValue;

        foreach (var baseline in corridor.Baselines)
        {
            if (baseline.StartStation < minStation)
                minStation = baseline.StartStation;
            if (baseline.EndStation > maxStation)
                maxStation = baseline.EndStation;
        }

        var startStation = minStation == double.MaxValue ? 0.0 : minStation;
        var endStation = maxStation == double.MinValue ? 0.0 : maxStation;

        return new CivilCorridorProperties()
        {
            Name = corridor.Name,
            Description = corridor.Description ?? string.Empty,
            StartStation = startStation,
            EndStation = endStation,
            Length = endStation - startStation,

        };
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public double StartStation { get; init; }

    /// <inheritdoc />
    public double EndStation { get; init; }

    /// <inheritdoc />
    public double Length { get; init; }

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilCorridorProperties"/>
    /// </summary>
    private CivilCorridorProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this corridor properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilCorridorProperties Duplicate()
    {
        return new CivilCorridorProperties()
        {
            Name = this.Name,
            Description = this.Description,
            StartStation = this.StartStation,
            EndStation = this.EndStation,
            Length = this.Length,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Corridor Properties: {this.Name} (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Length: {this.Length:F2})";
    }
}
