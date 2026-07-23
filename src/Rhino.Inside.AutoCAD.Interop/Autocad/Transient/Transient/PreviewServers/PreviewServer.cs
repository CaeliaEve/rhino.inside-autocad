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

            // Guard clause: skip if entity is null or already disposed
            if (autoCadEntity == null || autoCadEntity.IsDisposed)
            {
                continue;
            }

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
    ///  <remarks>
    /// TransientManager can throw if the entity was already erased or disposed, such
    /// as during closing of the application when we try to clear and dispose all
    /// entities. In those cases, so we still need to dispose the entities
    /// if disposeEntities is true.
    /// </remarks>
    /// <param name="entities">The entities to remove from the transient manager.</param>
    /// <param name="disposeEntities">If true, disposes the entities after removal.</param>
    private void RemoveTransientEntities(IEnumerable<IEntity> entities,
        bool disposeEntities = false)
    {
        try
        {
            var transientManager =
                TransientManager
                    .CurrentTransientManager; //Throws here in Civil Application Shutdown

            foreach (var entity in entities)
            {
                var autoCadEntity = entity.Unwrap();

                transientManager.EraseTransient(autoCadEntity, _emptyInterCollection);

                if (disposeEntities)
                {
                    autoCadEntity.Dispose();
                }
            }
        }
        catch (Exception e)
        {
            foreach (var entity in entities)
            {
                var autoCadEntity = entity.Unwrap();

                if (disposeEntities)
                {
                    autoCadEntity.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Removes transient elements from display but keeps them in the register for later re-use.
    /// Used for visibility toggling (preview on/off).
    /// </summary>
    public void ClearServer()
    {
        foreach (var entities in this.ObjectRegister)
        {
            this.RemoveTransientEntities(entities);
        }
    }

    /// <summary>
    /// Removes all transient elements and disposes the underlying AutoCAD entities.
    /// Used during application shutdown to ensure clean disposal.
    /// </summary>
    public void ClearAndDisposeAll()
    {
        System.Diagnostics.Debug.WriteLine("PreviewServer.ClearAndDisposeAll() - disposing entities");

        foreach (var entities in this.ObjectRegister)
        {
            this.RemoveTransientEntities(entities, disposeEntities: true);
        }

        System.Diagnostics.Debug.WriteLine("PreviewServer.ClearAndDisposeAll() - complete");
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
            this.RemoveTransientEntities(entities, disposeEntities: true);
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
                _previewSettings.ApplyTo(entity);
            }

            this.AddTransientEntities(entities);
        }
    }
}