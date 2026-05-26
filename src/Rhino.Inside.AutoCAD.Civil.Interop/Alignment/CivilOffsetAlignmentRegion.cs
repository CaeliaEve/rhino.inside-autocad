using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents an offset alignment region.
/// </summary>
public class CivilOffsetAlignmentRegion : AutocadWrapperBase<AlignmentRegion>, ICivilOffsetAlignmentRegion
{
    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Offset { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilOffsetAlignmentRegion"/>.
    /// </summary>
    public CivilOffsetAlignmentRegion(AlignmentRegion alignmentRegion) : base(alignmentRegion)
    {
        this.StartStation = alignmentRegion.StartStation;
        this.EndStation = alignmentRegion.EndStation;
        this.Offset = alignmentRegion.Offset;
    }

    /// <summary>
    /// Returns a string representation of this region.
    /// </summary>
    public override string ToString()
    {
        return $"Region: Sta {this.StartStation:F2}-{this.EndStation:F2}, Offset={this.Offset:F2}";
    }
}
