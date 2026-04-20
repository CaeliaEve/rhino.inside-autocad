using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a baseline extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This wrapper captures baseline data at construction time from a <see cref="Baseline"/>.
/// </remarks>
public class CivilCorridorBaselineWrapper : AutocadWrapperBase<Baseline>, ICivilCorridorBaseline
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IObjectId AlignmentId { get; }

    /// <inheritdoc />
    public IObjectId ProfileId { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public int RegionCount { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCorridorBaselineWrapper"/>
    /// from a Civil 3D Baseline.
    /// </summary>
    /// <param name="baseline">The baseline to extract data from.</param>
    public CivilCorridorBaselineWrapper(Baseline baseline) : base(baseline)
    {
        this.Name = baseline.Name ?? string.Empty;
        this.AlignmentId = new AutocadObjectIdWrapper(baseline.AlignmentId);
        this.ProfileId = new AutocadObjectIdWrapper(baseline.ProfileId);
        this.StartStation = baseline.StartStation;
        this.EndStation = baseline.EndStation;
        this.RegionCount = baseline.BaselineRegions.Count;
    }

    /// <summary>
    /// Creates a duplicate of this baseline wrapper.
    /// </summary>
    /// <returns>A new instance with the same wrapped object.</returns>
    public CivilCorridorBaselineWrapper ShallowClone()
    {
        return new CivilCorridorBaselineWrapper(_wrappedAutocadObject);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Corridor Baseline: {this.Name} (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Regions: {this.RegionCount})";
    }
}
