namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an AutoCAD layer definition (LayerTableRecord) with its visual properties.
/// </summary>
/// <remarks>
/// Wraps an AutoCAD LayerTableRecord to expose layer properties such as color, line type,
/// and lock state. Used extensively by Grasshopper layer components including
/// AutocadLayerComponent, GetAutocadLayersComponent, and CreateAutocadLayerComponent.
/// Layers control the visual appearance and organization of entities in a drawing.
/// </remarks>
/// <seealso cref="ILayerRegister"/>
/// <seealso cref="INamedDbObject"/>
public interface IAutocadLayerTableRecord : INamedDbObject
{
    /// <summary>
    /// Gets the default <see cref="IColor"/> for entities on this layer.
    /// </summary>
    /// <remarks>
    /// Entities with color set to "ByLayer" will display using this color.
    /// The color can be an ACI (AutoCAD Color Index) value or a true color.
    /// </remarks>
    IColor Color { get; }

    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the line type assigned to this layer.
    /// </summary>
    IObjectId LineTypeId { get; }

    /// <summary>
    /// Gets a value indicating whether this layer is locked.
    /// </summary>
    /// <remarks>
    /// Entities on a locked layer are visible but cannot be selected or modified.
    /// This is useful for protecting reference geometry while allowing it to remain visible.
    /// </remarks>
    bool IsLocked { get; }
}