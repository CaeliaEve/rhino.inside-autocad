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
    public string LabelType { get; }

    /// <summary>
    /// Initializes a new instance of the alignment label wrapper.
    /// </summary>
    public CivilFeatureLabelWrapperBase(T featureLabel) : base(featureLabel)
    {
        this.Location = featureLabel.LabelLocation.ToRhinoPoint3d();
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
