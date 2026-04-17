namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a label group from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// ProfileView label groups contain collections of labels displayed
/// within the profile view, such as grade breaks or station labels.
/// </remarks>
public interface ICivilProfileViewLabelGroup
{
    /// <summary>
    /// Gets the type of the label group as a string.
    /// </summary>
    /// <value>
    /// Common values: "MajorStation", "MinorStation", "GradeBreak",
    /// "HorizontalGeometry", "VerticalGeometry", etc.
    /// </value>
    string LabelGroupType { get; }

    /// <summary>
    /// Gets the name of the style applied to this label group.
    /// </summary>
    string StyleName { get; }

    /// <summary>
    /// Gets the number of labels in this group.
    /// </summary>
    int LabelCount { get; }

    /// <summary>
    /// Gets a value indicating whether the label group is visible.
    /// </summary>
    bool IsVisible { get; }
}
