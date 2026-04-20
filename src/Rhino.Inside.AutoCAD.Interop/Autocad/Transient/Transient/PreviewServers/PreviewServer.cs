using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IPreviewServer"/>
public class PreviewServer : IPreviewServer
{
    private readonly IGeometryPreviewSettings _previewSettings;
    private readonly IGeometryPreviewSettings _selectedPreviewSettings;
    private readonly IPreviewGeometryConverter _previewGeometryConverter;
    private readonly int _subDrawingMode = 0;
    private readonly IntegerCollection _emptyInterCollection = [];
    private readonly TransientDrawingMode _transientDrawingMode = TransientDrawingMode.Main;

    /// <inheritdoc/>
    public IObjectRegister ObjectRegister { get; }

    /// <summary>
    /// Constructs a new <see cref="IPreviewServer"/>
    /// </summary>
    public PreviewServer(IGeometryPreviewSettings previewSettings, IGeometryPreviewSettings selectedPreviewSettings,
        IPreviewGeometryConverter previewGeometryConverter)
    {
        _previewSettings = previewSettings;
        _selectedPreviewSettings = selectedPreviewSettings;
        _previewGeometryConverter = previewGeometryConverter;
        this.ObjectRegister = new ObjectRegister();
    }

    /// <summary>
    /// Adds the transient representation of an entity in AutoCAD.
    /// </summary>
    private void AddTransientEntities(IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            var autoCadEntity = entity.Unwrap();

            var transientManager = TransientManager.CurrentTransientManager;

            if (transientManager.AddTransient(autoCadEntity, _transientDrawingMode,
                    _subDrawingMode, _emptyInterCollection) == false)
            {
                LoggerService.Instance.LogMessage("Unable to create Transient element");
            }
        }
    }

    /// <summary>
    /// Removes the transient representation of an entity in AutoCAD.
    /// </summary>
    private void RemoveTransientEntities(IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            var autoCadEntity = entity.Unwrap();

            var transientManager = TransientManager.CurrentTransientManager;

            transientManager.EraseTransient(autoCadEntity, _emptyInterCollection);
        }
    }

    /// <summary>
    /// Updates the transient elements visibility based on the current state.
    /// </summary>
    public void ClearServer()
    {
        foreach (var entities in this.ObjectRegister)
        {
            this.RemoveTransientEntities(entities);
        }
    }

    /// <summary>
    /// Updates the transient elements visibility based on the current state.
    /// </summary>
    public void PopulateServer()
    {
        foreach (var entities in this.ObjectRegister)
        {
            this.AddTransientEntities(entities);
        }
    }

    /// <inheritdoc/>
    public void AddObject(Guid rhinoObjectId, IRhinoConvertibleSet rhinoConvertibleSet, bool selected)
    {
        if (rhinoConvertibleSet.Any)
        {
            var settings = selected ? _selectedPreviewSettings : _previewSettings;

            var entities = _previewGeometryConverter.Convert(rhinoConvertibleSet, settings);

            this.ObjectRegister.RegisterObject(rhinoObjectId, entities);

            this.AddTransientEntities(entities);
        }
    }

    /// <inheritdoc/>
    public void RemoveObject(Guid rhinoObjectId)
    {
        if (this.ObjectRegister.TryGetObject(rhinoObjectId, out var entities))
        {
            this.ObjectRegister.RemoveObject(rhinoObjectId);
            this.RemoveTransientEntities(entities);
        }
    }

    /// <summary>
    /// Applies the preview settings to the given entity.
    /// </summary>
    private void ApplySettings(IEntity entity, IGeometryPreviewSettings previewSettings)
    {
        var autocadEntity = entity.Unwrap();

        var materialId = previewSettings.MaterialId.Unwrap();

        autocadEntity.ColorIndex = previewSettings.ColorIndex;

        autocadEntity.LineWeight = LineWeight.LineWeight050;

        autocadEntity.Transparency = new Transparency(previewSettings.Transparency);

        if (materialId.IsValid)
        {
            autocadEntity.MaterialId = materialId;
        }
    }

    /// <inheritdoc />
    public void DeselectAll()
    {
        foreach (var entities in this.ObjectRegister)
        {
            this.RemoveTransientEntities(entities);

            foreach (var entity in entities)
            {
                this.ApplySettings(entity, _previewSettings);
            }

            this.AddTransientEntities(entities);
        }
    }
}