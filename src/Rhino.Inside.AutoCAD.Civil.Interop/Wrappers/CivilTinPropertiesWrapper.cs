using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps general statistics extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted surface property information.
/// The data is captured at construction time from a <see cref="TinSurface"/>.
/// </remarks>
public class CivilTinPropertiesWrapper : ICivilTinProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public double MinimumElevation { get; }

    /// <inheritdoc />
    public double MaximumElevation { get; }

    /// <inheritdoc />
    public double MinimumX { get; }

    /// <inheritdoc />
    public double MaximumX { get; }

    /// <inheritdoc />
    public double MinimumY { get; }

    /// <inheritdoc />
    public double MaximumY { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilTinPropertiesWrapper"/>
    /// from a Civil 3D TIN surface.
    /// </summary>
    /// <param name="tinSurface">The TIN surface to extract properties from.</param>
    public CivilTinPropertiesWrapper(TinSurface tinSurface)
    {
        Name = tinSurface.Name;

        var generalProps = tinSurface.GetGeneralProperties();
        MinimumElevation = generalProps.MinimumElevation;
        MaximumElevation = generalProps.MaximumElevation;
        MinimumX = generalProps.MinimumCoordinateX;
        MaximumX = generalProps.MaximumCoordinateX;
        MinimumY = generalProps.MinimumCoordinateY;
        MaximumY = generalProps.MaximumCoordinateY;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilTinPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilTinPropertiesWrapper(
        string name,
        double minimumElevation,
        double maximumElevation,
        double minimumX,
        double maximumX,
        double minimumY,
        double maximumY)
    {
        Name = name;
        MinimumElevation = minimumElevation;
        MaximumElevation = maximumElevation;
        MinimumX = minimumX;
        MaximumX = maximumX;
        MinimumY = minimumY;
        MaximumY = maximumY;
    }

    /// <summary>
    /// Creates a duplicate of this TIN properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilTinPropertiesWrapper Duplicate()
    {
        return new CivilTinPropertiesWrapper(
            Name,
            MinimumElevation,
            MaximumElevation,
            MinimumX,
            MaximumX,
            MinimumY,
            MaximumY);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"TIN Properties: {Name} (Elev: {MinimumElevation:F2} - {MaximumElevation:F2})";
    }
}
