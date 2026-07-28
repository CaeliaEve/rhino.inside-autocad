using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// A base class for <see cref="IRhinoConvertibleTyped{T}"/> types.
/// </summary>
public abstract class RhinoConvertibleBase<TRhinoType> : IRhinoConvertibleTyped<TRhinoType>
    where TRhinoType : Rhino.Geometry.GeometryBase
{
    /// <inheritdoc />
    public TRhinoType RhinoGeometry { get; }

    /// <summary>
    /// Constructs a new <see cref="RhinoConvertibleBase{TRhinoType}"/> instance.
    /// </summary>
    protected RhinoConvertibleBase(TRhinoType rhinoGeometry)
    {
        this.RhinoGeometry = rhinoGeometry;
    }

    /// <summary>
    /// Converts the Rhino geometry to AutoCAD entities.
    /// </summary>
    protected abstract List<IEntity> ConvertGeometry(IAutocadTransactionManager autocadTransactionManager);

    /// <inheritdoc />
    public List<IEntity> Convert(IAutocadTransactionManager autocadTransactionManager,
        IGeometryPreviewSettings previewSettings)
    {
        var converted = this.ConvertGeometry(autocadTransactionManager);

        foreach (var convertedEntity in converted)
        {
            previewSettings.ApplyTo(convertedEntity);
        }
        return converted;
    }

    /// <inheritdoc />
    public List<IEntity> Convert(IAutocadTransactionManager autocadTransactionManager)
    {
        return this.ConvertGeometry(autocadTransactionManager);
    }
}