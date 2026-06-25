namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an RGBA color used for entity and layer visualization.
/// </summary>
/// <remarks>
/// Provides a platform-independent color representation for converting between AutoCAD
/// colors (ACI or true color) and Rhino/Grasshopper colors. Used by layer and entity
/// wrappers such as <see cref="IAutocadLayerTableRecord"/> to expose color properties.
/// </remarks>
/// <seealso cref="IAutocadLayerTableRecord.Color"/>
public interface IColor
{
    /// <summary>
    /// Gets the red component of the color (0-255).
    /// </summary>
    byte Red { get; }

    /// <summary>
    /// Gets the green component of the color (0-255).
    /// </summary>
    byte Green { get; }

    /// <summary>
    /// Gets the blue component of the color (0-255).
    /// </summary>
    byte Blue { get; }

    /// <summary>
    /// Gets the alpha (opacity) component of the color (0-255).
    /// </summary>
    /// <remarks>
    /// A value of 255 represents fully opaque; 0 represents fully transparent.
    /// AutoCAD entities typically use fully opaque colors.
    /// </remarks>
    byte Alpha { get; }

    /// <summary>
    /// Determines whether this color is equal to another color based on RGBA components.
    /// </summary>
    bool IsEqualTo(IColor other);
}

/// <summary>
/// Represents an RGBA color used for entity and layer visualization.
/// </summary>
/// <remarks>
/// Provides a platform-independent color representation for converting between AutoCAD
/// colors (ACI or true color) and Rhino/Grasshopper colors. Used by layer and entity
/// wrappers such as <see cref="IAutocadLayerTableRecord"/> to expose color properties.
/// </remarks>
/// <seealso cref="IAutocadLayerTableRecord.Color"/>
public interface IAutocadColor
{
    /// <summary>
    /// Gets the AutoCAD Color Index (ACI) value.
    /// Special values: 256 = ByLayer, 0 = ByBlock, 1-255 = standard ACI colors.
    /// </summary>
    short ColorIndex { get; }

    /// <summary>
    /// Gets a value indicating whether this color is set to ByLayer.
    /// </summary>
    bool IsByLayer { get; }

    /// <summary>
    /// Gets a value indicating whether this color is set to ByBlock.
    /// </summary>
    bool IsByBlock { get; }

    /// <summary>
    /// Gets the true color displayed in Autocad.
    /// </summary>
    IColor ResolveColor(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Determines whether this color is equal to another.
    /// </summary>
    bool IsEqualTo(IAutocadColor other);
}