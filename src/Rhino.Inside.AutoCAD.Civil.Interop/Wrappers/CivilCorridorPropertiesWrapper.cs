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
public class CivilCorridorPropertiesWrapper : ICivilCorridorProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int BaselineCount { get; }

    /// <inheritdoc />
    public int SurfaceCount { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCorridorPropertiesWrapper"/>
    /// from a Civil 3D Corridor.
    /// </summary>
    /// <param name="corridor">The corridor to extract properties from.</param>
    public CivilCorridorPropertiesWrapper(Corridor corridor)
    {
        Name = corridor.Name;
        Description = corridor.Description ?? string.Empty;

        // Calculate corridor extents from baselines
        double minStation = double.MaxValue;
        double maxStation = double.MinValue;

        foreach (Baseline baseline in corridor.Baselines)
        {
            if (baseline.StartStation < minStation)
                minStation = baseline.StartStation;
            if (baseline.EndStation > maxStation)
                maxStation = baseline.EndStation;
        }

        StartStation = minStation == double.MaxValue ? 0.0 : minStation;
        EndStation = maxStation == double.MinValue ? 0.0 : maxStation;
        Length = EndStation - StartStation;

        BaselineCount = corridor.Baselines.Count;
        SurfaceCount = corridor.CorridorSurfaces.Count;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCorridorPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilCorridorPropertiesWrapper(
        string name,
        string description,
        double startStation,
        double endStation,
        double length,
        int baselineCount,
        int surfaceCount)
    {
        Name = name;
        Description = description;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        BaselineCount = baselineCount;
        SurfaceCount = surfaceCount;
    }

    /// <summary>
    /// Creates a duplicate of this corridor properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilCorridorPropertiesWrapper Duplicate()
    {
        return new CivilCorridorPropertiesWrapper(
            Name,
            Description,
            StartStation,
            EndStation,
            Length,
            BaselineCount,
            SurfaceCount);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Corridor Properties: {Name} (Sta: {StartStation:F2} - {EndStation:F2}, Length: {Length:F2}, Baselines: {BaselineCount})";
    }
}
