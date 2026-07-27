using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IGeometryPreviewSettings"/>
public class GeometryPreviewSettings : IGeometryPreviewSettings
{
    private readonly string _materialBaseName;

    private int _colorIndex;

    /// <inheritdoc/>
    /// <remarks>
    /// Clamped to the range AutoCAD accepts, so a color index typed into the user settings
    /// file by hand cannot make <see cref="EntityColor"/> throw somewhere far from here.
    /// </remarks>
    public int ColorIndex
    {
        get => _colorIndex;
        set
        {
            var colorIndex = Clamp(value);

            if (colorIndex == _colorIndex)
                return;

            _colorIndex = colorIndex;

            // The material held here was created for the previous color and is named after
            // it, so it is dropped until CreateMaterial makes one for the new color. Previews
            // drawn in the meantime skip the material rather than use the wrong one.
            this.MaterialId = AutocadObjectIdWrapper.DefaultId;
        }
    }

    /// <inheritdoc/>
    public byte Transparency { get; }

    /// <inheritdoc/>
    public IObjectId MaterialId { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// The color index is part of the name because <see cref="CreateMaterial"/> reuses any
    /// material already in the document's dictionary under this name. Were the name fixed, a
    /// color the user changed would never reach shaded previews in a document the old
    /// material had already been created in.
    /// </remarks>
    public string MaterialName => $"{_materialBaseName}.{this.ColorIndex}";

    /// <summary>
    /// Constructs a new <see cref="GeometryPreviewSettings"/>
    /// </summary>
    /// <param name="transparency">The transparency to draw previews with.</param>
    /// <param name="materialBaseName">
    /// The name of the preview material, which <see cref="MaterialName"/> qualifies with the
    /// current <see cref="ColorIndex"/>.
    /// </param>
    /// <param name="colorIndex">The AutoCAD Color Index to draw previews in.</param>
    public GeometryPreviewSettings(byte transparency, string materialBaseName, int colorIndex)
    {
        _materialBaseName = materialBaseName;
        _colorIndex = Clamp(colorIndex);
        this.Transparency = transparency;
        this.MaterialId = AutocadObjectIdWrapper.DefaultId;
    }

    /// <summary>
    /// Returns the given color index constrained to the range of AutoCAD Color Index values
    /// which name a color.
    /// </summary>
    private static int Clamp(int colorIndex) =>
        Math.Min(ApplicationConstants.MaxAciColorIndex,
            Math.Max(ApplicationConstants.MinAciColorIndex, colorIndex));

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

                this.MaterialId = new AutocadObjectIdWrapper(existingMaterialId);
                return true;
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
}