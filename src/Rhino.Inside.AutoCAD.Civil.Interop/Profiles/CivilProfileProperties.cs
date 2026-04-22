using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

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
    private readonly Profile _profile;

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public ICivilStationPoint Start { get; }

    /// <inheritdoc />
    public ICivilStationPoint End { get; }

    /// <inheritdoc />
    public double MinElevation { get; init; }

    /// <inheritdoc />
    public double MaxElevation { get; init; }

    /// <inheritdoc />
    public CivilProfileType ProfileType { get; init; }

    /// <inheritdoc />
    public IObjectId ParentAlignmentId { get; }

    /// <inheritdoc />
    public INamedId Style { get; init; } = NamedId.Empty;

    /// <summary>
    /// Initializes a instance of <see cref="CivilProfileProperties"/>
    /// </summary>
    public CivilProfileProperties(Profile profile)
    {
        _profile = profile;
        var startElevation = profile.ElevationAt(profile.StartingStation);
        var endElevation = profile.ElevationAt(profile.EndingStation);

        this.Name = profile.Name;
        this.Description = profile.Description ?? string.Empty;
        this.Start = new CivilStationPoint(profile.StartingStation, startElevation);
        this.End = new CivilStationPoint(profile.EndingStation, endElevation);
        this.MinElevation = profile.ElevationMin;
        this.MaxElevation = profile.ElevationMax;
        this.ProfileType = profile.ProfileType.ToRhinoInsideProfileType();
        this.Style = new NamedId(profile.StyleName, profile.StyleId);
        this.ParentAlignmentId = new AutocadObjectIdWrapper(profile.AlignmentId);

    }

    /// <summary>
    /// Creates a duplicate of this profile properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileProperties Duplicate()
    {
        return new CivilProfileProperties(_profile);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Properties: {this.Name} (Start:[{this.Start}] - End:[{this.End}])";
    }
}
