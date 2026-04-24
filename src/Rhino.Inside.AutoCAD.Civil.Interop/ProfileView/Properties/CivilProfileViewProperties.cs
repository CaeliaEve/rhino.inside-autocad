using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted ProfileView property information.
/// The data is captured at construction time from a <see cref="ProfileView"/>.
/// </remarks>
public record CivilProfileViewProperties : ICivilProfileViewProperties
{
    private readonly ProfileView _profileView;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public Interval StationRange { get; }

    /// <inheritdoc />
    public Interval ElevationRange { get; }

    /// <inheritdoc />
    public INamedId Style { get; }

    /// <inheritdoc />
    public IObjectId ProfileViewId { get; }

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilProfileViewProperties"/>
    /// </summary>
    public CivilProfileViewProperties(ProfileView profileView)
    {
        _profileView = profileView;

        var stationRange = new Interval(profileView.StationStart, profileView.StationEnd);
        var elevationRange = new Interval(profileView.ElevationMin, profileView.ElevationMax);

        this.Name = profileView.Name;
        this.Description = profileView.Description ?? string.Empty;
        this.StationRange = stationRange;
        this.ElevationRange = elevationRange;

        this.Style = new NamedId(profileView.StyleName, profileView.StyleId);
        this.ProfileViewId = new AutocadObjectIdWrapper(_profileView.Id);
    }

    /// <inheritdoc />
    public IProfileViewCoordinateSystem GetCoordinateSystem(IAutocadTransactionManager transactionManager)
    {
        return
              new ProfileViewCoordinateSystem(_profileView, transactionManager);
    }

    /// <inheritdoc />
    public ICivilProfileViewProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription)
    {
        var profileView = transactionManager.Unwrap()
            .GetObject(_profileView.Id, OpenMode.ForWrite) as ProfileView;

        if (profileView == null)
        {
            return this;
        }

        profileView.Name = newName;
        profileView.Description = newDescription;

        return new CivilProfileViewProperties(profileView);
    }

    /// <summary>
    /// Creates a duplicate of this ProfileView properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewProperties Duplicate()
    {
        return new CivilProfileViewProperties(_profileView);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Properties: {this.Name} (Sta: {this.StationRange.T0:F2} - {this.StationRange.T1:F2}, Elev: {this.ElevationRange.T0:F2} - {this.ElevationRange.T1:F2})";
    }
}
