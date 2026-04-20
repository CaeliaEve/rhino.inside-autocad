using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a Civil 3D LabelGroup from an Alignment.
/// </summary>
/// <remarks>
/// Works with all alignment label group types (Station, Cant, DesignSpeed,
/// GeometryPoint, StationEquation, Superelevation, VerticalGeometryPoint).
/// Provides access to the underlying Civil 3D LabelGroup object via the
/// <see cref="CivilInteropConverter.Unwrap(ICivilAlignmentLabelGroup)"/> extension method.
/// </remarks>
public class CivilAlignmentLabelGroupWrapper : AutocadEntityWrapper, ICivilAlignmentLabelGroup
{
    private readonly AlignmentLabelGroup _labelGroup;

    /// <inheritdoc/>
    public string LabelGroupType { get; }

    /// <inheritdoc/>
    public string StyleName { get; }

    /// <inheritdoc/>
    public int LabelCount { get; }

    /// <inheritdoc/>
    public double RangeStart { get; }

    /// <inheritdoc/>
    public double RangeEnd { get; }

    /// <inheritdoc/>
    public bool RangeStartFromFeature { get; }

    /// <inheritdoc/>
    public bool RangeEndFromFeature { get; }

    /// <inheritdoc/>
    public bool IsVisible { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentLabelGroupWrapper"/> class.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D <see cref="LabelGroup"/> to wrap.</param>
    public CivilAlignmentLabelGroupWrapper(AlignmentLabelGroup labelGroup) : base(labelGroup)
    {
        _labelGroup = labelGroup;
        this.LabelGroupType = labelGroup.GetType().Name;
        this.StyleName = labelGroup.StyleName ?? "";
        this.LabelCount = (int)labelGroup.SubEntityCount;
        this.RangeStart = labelGroup.RangeStart;
        this.RangeEnd = labelGroup.RangeEnd;
        this.RangeStartFromFeature = labelGroup.RangeStartFromFeature;
        this.RangeEndFromFeature = labelGroup.RangeEndFromFeature;
        this.IsVisible = labelGroup.Visible;

    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilAlignmentLabelGroupWrapper(_labelGroup);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Label Group [{this.LabelGroupType}] (Style: {this.StyleName}, Count: {this.LabelCount})";
    }
}
