using Autodesk.AutoCAD.DatabaseServices;
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
    public string Name { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; private set; } = string.Empty;

    /// <inheritdoc />
    public ICivilStationPoint Start { get; }

    /// <inheritdoc />
    public ICivilStationPoint End { get; }

    /// <inheritdoc />
    public double MinElevation { get; private set; }

    /// <inheritdoc />
    public double MaxElevation { get; private set; }

    /// <inheritdoc />
    public CivilProfileType ProfileType { get; private set; }

    /// <inheritdoc />
    public IObjectId ParentAlignmentId { get; }

    /// <inheritdoc />
    public INamedId Style { get; private set; } = NamedId.Empty;

    /// <inheritdoc />
    public IObjectId ProfileId { get; }

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
        this.ProfileId = new AutocadObjectIdWrapper(profile.Id);
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
    public ICivilProfileProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription)
    {
        var profile = transactionManager.Unwrap()
            .GetObject(_profile.Id, OpenMode.ForWrite) as Profile;

        if (profile == null)
        {
            return this;
        }

        profile.Name = newName;
        profile.Description = newDescription;

        return new CivilProfileProperties(profile);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Properties: {this.Name} (Start:[{this.Start}] - End:[{this.End}])";
    }
}
