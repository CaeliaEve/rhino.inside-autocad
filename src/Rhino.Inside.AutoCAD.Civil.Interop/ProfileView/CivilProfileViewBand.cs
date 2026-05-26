using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps data extracted from a Civil 3D ProfileView band.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted band information.
/// </remarks>
public class CivilProfileViewBand : AutocadWrapperBase<ProfileViewBandItem>, ICivilProfileViewBand
{
    private readonly ProfileViewBandItem _bandItem;

    /// <inheritdoc />
    public CivilBandType BandType { get; }

    /// <inheritdoc />
    public IObjectId StyleId { get; }

    /// <inheritdoc />
    public CivilBandLocationType Location { get; }

    /// <inheritdoc />
    public IObjectId DataSourceId { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileViewBand"/>
    /// with the specified values.
    /// </summary>
    public CivilProfileViewBand(ProfileViewBandItem bandItem, int index) : base(bandItem)
    {
        _bandItem = bandItem;

        this.BandType = bandItem.BandType.ToRhinoInsideBandType();

        this.StyleId = new AutocadObjectIdWrapper(bandItem.BandStyleId);

        this.Location = bandItem.Location.ToRhinoInsideBandLocationType();

        this.DataSourceId = new AutocadObjectIdWrapper(bandItem.DataSourceId);

        this.Index = index;
    }

    /// <summary>
    /// Creates a duplicate of this band wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewBand ShallowClone()
    {
        return new CivilProfileViewBand(_bandItem, this.Index);

    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Band: {this.BandType.ToString()} ({this.BandType}, {this.Location})";
    }
}
