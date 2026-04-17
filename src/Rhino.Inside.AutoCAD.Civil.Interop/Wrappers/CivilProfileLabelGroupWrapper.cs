using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a Civil 3D ProfileLabelGroup.
/// </summary>
/// <remarks>
/// Provides access to the underlying Civil 3D ProfileLabelGroup object via the
/// <see cref="CivilInteropConverter.Unwrap(ICivilProfileLabelGroup)"/> extension method.
/// </remarks>
public class CivilProfileLabelGroupWrapper : AutocadEntityWrapper, ICivilProfileLabelGroup
{
    private readonly ProfileLabelGroup _labelGroup;

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
    /// Initializes a new instance of the <see cref="CivilProfileLabelGroupWrapper"/> class.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D <see cref="ProfileLabelGroup"/> to wrap.</param>
    public CivilProfileLabelGroupWrapper(ProfileLabelGroup labelGroup) : base(labelGroup)
    {
        _labelGroup = labelGroup;
        this.LabelGroupType = labelGroup.GetType().Name;
        this.StyleName = labelGroup.StyleName ?? "";
        this.LabelCount = (int)labelGroup.SubEntityCount;
        this.RangeStart = UnitConverter.ToRhinoLength(labelGroup.RangeStart);
        this.RangeEnd = UnitConverter.ToRhinoLength(labelGroup.RangeEnd);
        this.RangeStartFromFeature = labelGroup.RangeStartFromFeature;
        this.RangeEndFromFeature = labelGroup.RangeEndFromFeature;
        this.IsVisible = labelGroup.Visible;
    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilProfileLabelGroupWrapper(_labelGroup);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Profile Label Group [{this.LabelGroupType}] (Style: {this.StyleName}, Count: {this.LabelCount})";
    }
}
