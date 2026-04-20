using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted profile property information.
/// The data is captured at construction time from a <see cref="Profile"/>.
/// </remarks>
public record CivilProfileProperties : ICivilProfileProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilProfileProperties"/> by extracting
    /// data from a given <see cref="Profile"/>.
    /// </summary>
    /// <param name="profile">The profile to extract properties from.</param>
    /// <param name="parentAlignmentName">The name of the parent alignment.</param>
    public static CivilProfileProperties CreateFromProfile(Profile profile, string parentAlignmentName)
    {
        return new CivilProfileProperties()
        {
            Name = profile.Name,
            Description = profile.Description ?? string.Empty,
            StartStation = profile.StartingStation,
            EndStation = profile.EndingStation,
            MinElevation = profile.ElevationMin,
            MaxElevation = profile.ElevationMax,
            ProfileType = (CivilProfileType)profile.ProfileType,
            EntityCount = profile.Entities.Count,
            ParentAlignmentName = parentAlignmentName,
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
    public double MinElevation { get; init; }

    /// <inheritdoc />
    public double MaxElevation { get; init; }

    /// <inheritdoc />
    public CivilProfileType ProfileType { get; init; }

    /// <inheritdoc />
    public int EntityCount { get; init; }

    /// <inheritdoc />
    public string ParentAlignmentName { get; init; } = string.Empty;

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilProfileProperties"/>
    /// </summary>
    private CivilProfileProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this profile properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileProperties Duplicate()
    {
        return new CivilProfileProperties()
        {
            Name = this.Name,
            Description = this.Description,
            StartStation = this.StartStation,
            EndStation = this.EndStation,
            MinElevation = this.MinElevation,
            MaxElevation = this.MaxElevation,
            ProfileType = this.ProfileType,
            EntityCount = this.EntityCount,
            ParentAlignmentName = this.ParentAlignmentName,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Properties: {this.Name} (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Elev: {this.MinElevation:F2} - {this.MaxElevation:F2})";
    }
}
