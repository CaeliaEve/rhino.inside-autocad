using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps data extracted from a Civil 3D ProfileView band.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted band information.
/// </remarks>
public class CivilProfileViewBandWrapper : ICivilProfileViewBand
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
    /// Initializes a new instance of <see cref="CivilProfileViewBandWrapper"/>
    /// with the specified values.
    /// </summary>
    /// <param name="name">The name of the band.</param>
    /// <param name="bandType">The type of the band.</param>
    /// <param name="styleName">The name of the band style.</param>
    /// <param name="location">The location (Top or Bottom).</param>
    /// <param name="isVisible">Whether the band is visible.</param>
    public CivilProfileViewBandWrapper(
        string name,
        string bandType,
        string styleName,
        string location,
        bool isVisible)
    {
        Name = name;
        BandType = bandType;
        StyleName = styleName;
        Location = location;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Creates a duplicate of this band wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileViewBandWrapper Duplicate()
    {
        return new CivilProfileViewBandWrapper(
            Name,
            BandType,
            StyleName,
            Location,
            IsVisible);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ProfileView Band: {Name} ({BandType}, {Location})";
    }
}
