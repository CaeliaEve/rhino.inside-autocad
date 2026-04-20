using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a baseline region extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This wrapper captures baseline region data at construction time from a <see cref="BaselineRegion"/>.
/// </remarks>
public class CivilCorridorBaselineRegionWrapper : AutocadWrapperBase<BaselineRegion>, ICivilCorridorBaselineRegion
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IObjectId AssemblyId { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCorridorBaselineRegionWrapper"/>
    /// from a Civil 3D BaselineRegion.
    /// </summary>
    /// <param name="region">The baseline region to extract data from.</param>
    public CivilCorridorBaselineRegionWrapper(BaselineRegion region) : base(region)
    {
        this.Name = region.Name ?? string.Empty;

        this.AssemblyId = new AutocadObjectIdWrapper(region.AssemblyId);
        this.StartStation = region.StartStation;
        this.EndStation = region.EndStation;
        this.Length = this.EndStation - this.StartStation;
    }

    /// <summary>
    /// Creates a duplicate of this baseline region wrapper.
    /// </summary>
    /// <returns>A new instance with the same wrapped object.</returns>
    public CivilCorridorBaselineRegionWrapper ShallowClone()
    {
        return new CivilCorridorBaselineRegionWrapper(_wrappedAutocadObject);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Baseline Region: {this.Name} (Station: {this.StartStation:F2} - {this.EndStation:F2})";
    }
}
