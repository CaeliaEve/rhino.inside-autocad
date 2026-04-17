using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment property information.
/// The data is captured at construction time from an <see cref="Alignment"/>.
/// </remarks>
public class CivilAlignmentPropertiesWrapper : ICivilAlignmentProperties
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
    public int AlignmentType { get; }

    /// <inheritdoc />
    public int EntityCount { get; }

    /// <inheritdoc />
    public string SiteName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentPropertiesWrapper"/>
    /// from a Civil 3D Alignment.
    /// </summary>
    /// <param name="alignment">The alignment to extract properties from.</param>
    public CivilAlignmentPropertiesWrapper(Alignment alignment)
    {
        Name = alignment.Name;
        Description = alignment.Description ?? string.Empty;
        StartStation = alignment.StartingStation;
        EndStation = alignment.EndingStation;
        Length = alignment.Length;
        AlignmentType = (int)alignment.AlignmentType;
        EntityCount = alignment.Entities.Count;
        SiteName = alignment.SiteName ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilAlignmentPropertiesWrapper(
        string name,
        string description,
        double startStation,
        double endStation,
        double length,
        int alignmentType,
        int entityCount,
        string siteName)
    {
        Name = name;
        Description = description;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        AlignmentType = alignmentType;
        EntityCount = entityCount;
        SiteName = siteName;
    }

    /// <summary>
    /// Creates a duplicate of this alignment properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentPropertiesWrapper Duplicate()
    {
        return new CivilAlignmentPropertiesWrapper(
            Name,
            Description,
            StartStation,
            EndStation,
            Length,
            AlignmentType,
            EntityCount,
            SiteName);
    }

    /// <summary>
    /// Gets a human-readable description of the alignment type.
    /// </summary>
    public string AlignmentTypeName => AlignmentType switch
    {
        0 => "Centerline",
        1 => "Offset",
        2 => "CurbReturn",
        3 => "Rail",
        4 => "Miscellaneous",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Properties: {Name} (Sta: {StartStation:F2} - {EndStation:F2}, Length: {Length:F2})";
    }
}
