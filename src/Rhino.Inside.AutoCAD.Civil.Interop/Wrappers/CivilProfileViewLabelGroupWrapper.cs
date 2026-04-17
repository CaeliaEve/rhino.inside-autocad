using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps data extracted from a Civil 3D ProfileView label group.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted label group information.
/// </remarks>
public class CivilProfileViewLabelGroupWrapper : ICivilProfileViewLabelGroup
{
    /// <inheritdoc />
    public string LabelGroupType { get; }

    /// <inheritdoc />
    public string StyleName { get; }

    /// <inheritdoc />
    public int LabelCount { get; }

    /// <inheritdoc />
    public bool IsVisible { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileViewLabelGroupWrapper"/>
    /// with the specified values.
    /// </summary>
    /// <param name="labelGroupType">The type of the label group.</param>
    /// <param name="styleName">The name of the label style.</param>
    /// <param name="labelCount">The number of labels in this group.</param>
    /// <param name="isVisible">Whether the label group is visible.</param>
    public CivilProfileViewLabelGroupWrapper(
        string labelGroupType,
        string styleName,
        int labelCount,
        bool isVisible)
    {
        LabelGroupType = labelGroupType;
        StyleName = styleName;
        LabelCount = labelCount;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Creates a duplicate of this label group wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewLabelGroupWrapper Duplicate()
    {
        return new CivilProfileViewLabelGroupWrapper(
            LabelGroupType,
            StyleName,
            LabelCount,
            IsVisible);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Label Group: {LabelGroupType} ({LabelCount} labels)";
    }
}
