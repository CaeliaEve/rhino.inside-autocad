using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Base wrapper class for alignment label groups.
/// </summary>
public abstract class CivilAlignmentLabelGroupWrapperBase : ICivilAlignmentLabelGroup
{
    /// <inheritdoc />
    public string LabelGroupType { get; }

    /// <inheritdoc />
    public string StyleName { get; }

    /// <inheritdoc />
    public int LabelCount { get; }

    /// <summary>
    /// Initializes a new instance of the alignment label group wrapper.
    /// </summary>
    protected CivilAlignmentLabelGroupWrapperBase(
        string labelGroupType,
        string styleName,
        int labelCount)
    {
        LabelGroupType = labelGroupType;
        StyleName = styleName;
        LabelCount = labelCount;
    }

    /// <summary>
    /// Creates a duplicate of this label group wrapper.
    /// </summary>
    public abstract CivilAlignmentLabelGroupWrapperBase DuplicateBase();

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Label Group [{LabelGroupType}] (Style: {StyleName}, Count: {LabelCount})";
    }
}

/// <summary>
/// Wrapper for Civil 3D Alignment Cant label groups.
/// </summary>
public class CivilAlignmentCantLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentCantLabelGroup
{
    public CivilAlignmentCantLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentCantLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentCantLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Design Speed label groups.
/// </summary>
public class CivilAlignmentDesignSpeedLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentDesignSpeedLabelGroup
{
    public CivilAlignmentDesignSpeedLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentDesignSpeedLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentDesignSpeedLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Geometry Point label groups.
/// </summary>
public class CivilAlignmentGeometryPointLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentGeometryPointLabelGroup
{
    public CivilAlignmentGeometryPointLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentGeometryPointLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentGeometryPointLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Station Equation label groups.
/// </summary>
public class CivilAlignmentStationEquationLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentStationEquationLabelGroup
{
    public CivilAlignmentStationEquationLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentStationEquationLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentStationEquationLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Station label groups.
/// </summary>
public class CivilAlignmentStationLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentStationLabelGroup
{
    public CivilAlignmentStationLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentStationLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentStationLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Superelevation label groups.
/// </summary>
public class CivilAlignmentSuperelevationLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentSuperelevationLabelGroup
{
    public CivilAlignmentSuperelevationLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentSuperelevationLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentSuperelevationLabelGroupWrapper(StyleName, LabelCount);
}

/// <summary>
/// Wrapper for Civil 3D Alignment Vertical Geometry Point label groups.
/// </summary>
public class CivilAlignmentVerticalGeometryPointLabelGroupWrapper : CivilAlignmentLabelGroupWrapperBase, ICivilAlignmentVerticalGeometryPointLabelGroup
{
    public CivilAlignmentVerticalGeometryPointLabelGroupWrapper(string styleName, int labelCount)
        : base("AlignmentVerticalGeometryPointLabelGroup", styleName, labelCount) { }

    public override CivilAlignmentLabelGroupWrapperBase DuplicateBase() =>
        new CivilAlignmentVerticalGeometryPointLabelGroupWrapper(StyleName, LabelCount);
}
