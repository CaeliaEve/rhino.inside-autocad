namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// An interface which represents a wrapper for an AutoCAD Database.
/// </summary>
public interface IAutocadDatabase : IDisposable
{
    /// <summary>
    /// Returns the BlockTableId of this <see cref="IAutocadDatabase"/>.
    /// </summary>
    IObjectId BlockTableId { get; }

    /// <summary>
    /// Returns the LinetypeTableId of this <see cref="IAutocadDatabase"/>.
    /// </summary>
    IObjectId LinetypeTableId { get; }

    /// <summary>
    /// Returns the LayerTableId of this <see cref="IAutocadDatabase"/>.
    /// </summary>
    IObjectId LayerTableId { get; }

    /// <summary>
    /// Returns the LayoutDictionaryId of this <see cref="IAutocadDatabase"/>.
    /// </summary>
    IObjectId LayoutDictionaryId { get; }
}