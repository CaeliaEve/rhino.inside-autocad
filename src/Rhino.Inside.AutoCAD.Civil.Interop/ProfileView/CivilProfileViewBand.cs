using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps data extracted from a Civil 3D ProfileView band.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted band information.
/// </remarks>
public class CivilProfileViewBand : ICivilProfileViewBand
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string BandType { get; }

    /// <inheritdoc />
    public string StyleName { get; }

    /// <inheritdoc />
    public string Location { get; }

    /// <inheritdoc />
    public bool IsVisible { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileViewBand"/>
    /// with the specified values.
    /// </summary>
    /// <param name="name">The name of the band.</param>
    /// <param name="bandType">The type of the band.</param>
    /// <param name="styleName">The name of the band style.</param>
    /// <param name="location">The location (Top or Bottom).</param>
    /// <param name="isVisible">Whether the band is visible.</param>
    public CivilProfileViewBand(
        string name,
        string bandType,
        string styleName,
        string location,
        bool isVisible)
    {
        this.Name = name;
        this.BandType = bandType;
        this.StyleName = styleName;
        this.Location = location;
        this.IsVisible = isVisible;
    }

    /// <summary>
    /// Creates a duplicate of this band wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewBand Duplicate()
    {
        return new CivilProfileViewBand(
            this.Name,
            this.BandType,
            this.StyleName,
            this.Location,
            this.IsVisible);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Band: {this.Name} ({this.BandType}, {this.Location})";
    }
}
