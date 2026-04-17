using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted profile property information.
/// The data is captured at construction time from a <see cref="Profile"/>.
/// </remarks>
public class CivilProfilePropertiesWrapper : ICivilProfileProperties
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
    public double MinElevation { get; }

    /// <inheritdoc />
    public double MaxElevation { get; }

    /// <inheritdoc />
    public int ProfileType { get; }

    /// <inheritdoc />
    public int EntityCount { get; }

    /// <inheritdoc />
    public string ParentAlignmentName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfilePropertiesWrapper"/>
    /// from a Civil 3D Profile.
    /// </summary>
    /// <param name="profile">The profile to extract properties from.</param>
    /// <param name="parentAlignmentName">The name of the parent alignment.</param>
    public CivilProfilePropertiesWrapper(Profile profile, string parentAlignmentName)
    {
        Name = profile.Name;
        Description = profile.Description ?? string.Empty;
        StartStation = profile.StartingStation;
        EndStation = profile.EndingStation;
        MinElevation = profile.ElevationMin;
        MaxElevation = profile.ElevationMax;
        ProfileType = (int)profile.ProfileType;
        EntityCount = profile.Entities.Count;
        ParentAlignmentName = parentAlignmentName;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfilePropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilProfilePropertiesWrapper(
        string name,
        string description,
        double startStation,
        double endStation,
        double minElevation,
        double maxElevation,
        int profileType,
        int entityCount,
        string parentAlignmentName)
    {
        Name = name;
        Description = description;
        StartStation = startStation;
        EndStation = endStation;
        MinElevation = minElevation;
        MaxElevation = maxElevation;
        ProfileType = profileType;
        EntityCount = entityCount;
        ParentAlignmentName = parentAlignmentName;
    }

    /// <summary>
    /// Creates a duplicate of this profile properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfilePropertiesWrapper Duplicate()
    {
        return new CivilProfilePropertiesWrapper(
            Name,
            Description,
            StartStation,
            EndStation,
            MinElevation,
            MaxElevation,
            ProfileType,
            EntityCount,
            ParentAlignmentName);
    }

    /// <summary>
    /// Gets a human-readable description of the profile type.
    /// </summary>
    public string ProfileTypeName => ProfileType switch
    {
        0 => "ExistingGround",
        1 => "Layout",
        2 => "SuperimposedProfile",
        3 => "Quick",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Properties: {Name} (Sta: {StartStation:F2} - {EndStation:F2}, Elev: {MinElevation:F2} - {MaxElevation:F2})";
    }
}
