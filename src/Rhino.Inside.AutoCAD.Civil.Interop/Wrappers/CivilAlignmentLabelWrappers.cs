using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Base wrapper class for alignment labels.
/// </summary>
public class CivilFeatureLabelWrapperBase<T> : AutocadWrapperDisposableBase<T>, ICivilFeatureLabel where T : FeatureLabel
{

    /// <inheritdoc />
    public Point3d Location { get; }

    /// <inheritdoc />
    public double Rotation { get; }

    /// <inheritdoc />
    public string LabelType { get; }

    /// <summary>
    /// Initializes a new instance of the alignment label wrapper.
    /// </summary>
    public CivilFeatureLabelWrapperBase(T featureLabel) : base(featureLabel)
    {
        this.Location = featureLabel.LabelLocation.ToRhinoPoint3d();
        this.Rotation = featureLabel.RotationAngle;
        this.LabelType = featureLabel.GetType().Name;
    }

    /// <inheritdoc />
    public List<IEntity> ExtractTextEntities(IAutocadTransactionManager transactionManager)
    {
        var textParts = new List<IEntity>();

        var textComponentIds = _wrappedAutocadObject.GetTextComponentIds();

        foreach (ObjectId textCompId in textComponentIds)
        {
            if (textCompId.IsNull || textCompId.IsErased)
                continue;

            var textObjects = transactionManager.Unwrap()
                .GetObject(textCompId, OpenMode.ForRead) as Entity;

            textParts.Add(new AutocadEntityWrapper(textObjects));

        }

        return textParts;
    }

    /// <inheritdoc />
    public string GetStyleName(IAutocadTransactionManager transactionManager)
    {
        if (_wrappedAutocadObject.StyleId.IsNull || _wrappedAutocadObject.StyleId.IsErased)
            return string.Empty;

        try
        {
            var style = transactionManager.Unwrap()
                .GetObject(_wrappedAutocadObject.StyleId, OpenMode.ForRead) as DBObject;

            return style switch
            {
                LabelStyle labelStyle => labelStyle.Name,
                _ => style?.GetType().Name ?? string.Empty
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Feature Label: {this.LabelType}";
    }
}

/// <summary>
/// Wrapper for Civil 3D Alignment Curve labels.
/// </summary>
public class CivilAlignmentCurveLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentCurveLabel>, ICivilAlignmentCurveLabel
{
    public CivilAlignmentCurveLabelWrapper(AlignmentCurveLabel label)
        : base(label) { }
}

/// <summary>
/// Wrapper for Civil 3D Alignment Spiral labels.
/// </summary>
public class CivilAlignmentSpiralLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentSpiralLabel>, ICivilAlignmentSpiralLabel
{
    public CivilAlignmentSpiralLabelWrapper(AlignmentSpiralLabel label)
        : base(label) { }
}

/// <summary>
/// Wrapper for Civil 3D Alignment Tangent labels.
/// </summary>
public class CivilAlignmentTangentLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentTangentLabel>, ICivilAlignmentTangentLabel
{
    public CivilAlignmentTangentLabelWrapper(AlignmentTangentLabel label)
        : base(label) { }
}

/// <summary>
/// Wrapper for Civil 3D Alignment PI (Point of Intersection) labels.
/// </summary>
public class CivilAlignmentPILabelWrapper : CivilFeatureLabelWrapperBase<AlignmentPILabel>, ICivilAlignmentPILabel
{
    public CivilAlignmentPILabelWrapper(AlignmentPILabel label)
        : base(label) { }
}

/// <summary>
/// Wrapper for Civil 3D Alignment Indexed PI labels.
/// </summary>
public class CivilAlignmentIndexedPILabelWrapper : CivilFeatureLabelWrapperBase<AlignmentIndexedPILabel>, ICivilAlignmentIndexedPILabel
{
    public CivilAlignmentIndexedPILabelWrapper(AlignmentIndexedPILabel label)
        : base(label) { }
}
