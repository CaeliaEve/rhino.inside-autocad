using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IGeometryPreviewSettings"/>
public class GeometryPreviewSettings : IGeometryPreviewSettings
{
    /// <inheritdoc/>
    public int ColorIndex { get; }

    /// <inheritdoc/>
    public byte Transparency { get; }

    /// <inheritdoc/>
    public IObjectId MaterialId { get; private set; }

    /// <inheritdoc/>
    public string MaterialName { get; }

    /// <summary>
    /// Constructs a new <see cref="GeometryPreviewSettings"/>
    /// </summary>
    public GeometryPreviewSettings(byte transparency, string materialName, int colorIndex)
    {
        this.ColorIndex = colorIndex;
        this.Transparency = transparency;
        this.MaterialId = AutocadObjectIdWrapper.DefaultId;
        this.MaterialName = materialName;
    }

    /// <inheritdoc/>
    public void CreateMaterial(IAutocadDocument document)
    {
        var transactionManagerWrapper = document.CreateTransactionManager();

        _ = transactionManagerWrapper.PerformTask(() =>
        {
            var transactionManager = transactionManagerWrapper.Unwrap();

            using var dbDictionary =
                (DBDictionary)transactionManager.GetObject(document.AutocadDatabase.Unwrap().MaterialDictionaryId,
                    OpenMode.ForWrite);

            if (dbDictionary.Contains(this.MaterialName))
            {
                var existingMaterialId = dbDictionary.GetAt(this.MaterialName);

                // An erased entry can linger in the dictionary (e.g. after UNDO); fall
                // through to recreate the material so we never cache an erased id.
                if (existingMaterialId.IsErased == false)
                {
                    this.MaterialId = new AutocadObjectIdWrapper(existingMaterialId);
                    return true;
                }
            }

            var material = new Material
            {
                Name = this.MaterialName,
            };

            var materialColor =
                new MaterialColor(Method.Override, 1.0, new EntityColor(this.ColorIndex));

            material.Diffuse = new MaterialDiffuseComponent(materialColor, null);

            material.Ambient = materialColor;
            material.Specular =
                new MaterialSpecularComponent(materialColor, new MaterialMap(), 0.5);
            material.Opacity = new MaterialOpacityComponent(0.5, null);

            _ = dbDictionary.SetAt(material.Name, material);
            transactionManager.AddNewlyCreatedDBObject(material, true);

            this.MaterialId = new AutocadObjectIdWrapper(material.ObjectId);
            return true;
        });
    }

    /// <inheritdoc/>
    public void EnsureMaterial(IAutocadDocument document)
    {
        var materialId = this.MaterialId.Unwrap();

        if (materialId is { IsNull: false, IsValid: true, IsErased: false } &&
            materialId.Database == document.AutocadDatabase.Unwrap())
        {
            return;
        }

        this.CreateMaterial(document);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The cached preview material id can go stale (document switched or closed, UNDO past
    /// the material creation, or PURGE erasing the unreferenced material), so it is only
    /// applied when it is valid, not erased and belongs to the current working database.
    /// Applying the material is cosmetic and must never throw into the calling event handler.
    /// </remarks>
    public void ApplyTo(IEntity entity)
    {
        var autocadEntity = entity.Unwrap();

        autocadEntity.ColorIndex = this.ColorIndex;

        autocadEntity.LineWeight = LineWeight.LineWeight050;

        autocadEntity.Transparency = new Transparency(this.Transparency);

        try
        {
            var materialId = this.MaterialId.Unwrap();

            if (materialId is { IsNull: false, IsValid: true, IsErased: false } &&
                materialId.Database == HostApplicationServices.WorkingDatabase)
            {
                autocadEntity.MaterialId = materialId;
            }
        }
        catch (Autodesk.AutoCAD.Runtime.Exception exception)
        {
            LoggerService.Instance.LogMessage(
                $"Unable to apply preview material '{this.MaterialName}': {exception.Message}");
        }
    }
}