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
public record CivilTinSurfaceProperties : ICivilTinSurfaceProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilTinSurfaceProperties"/> by extracting
    /// data from a given <see cref="TinSurface"/>.
    /// </summary>
    public static CivilTinSurfaceProperties CreateFromTinSurface(TinSurface tinSurface)
    {
        var generalProps = tinSurface.GetGeneralProperties();

        return new CivilTinSurfaceProperties()
        {
            Name = tinSurface.Name,
            MinimumElevation = generalProps.MinimumElevation,
            MaximumElevation = generalProps.MaximumElevation,
            MinimumX = generalProps.MinimumCoordinateX,
            MaximumX = generalProps.MaximumCoordinateX,
            MinimumY = generalProps.MinimumCoordinateY,
            MaximumY = generalProps.MaximumCoordinateY,
        };
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public double MinimumElevation { get; init; }

    /// <inheritdoc />
    public double MaximumElevation { get; init; }

    /// <inheritdoc />
    public double MinimumX { get; init; }

    /// <inheritdoc />
    public double MaximumX { get; init; }

    /// <inheritdoc />
    public double MinimumY { get; init; }

    /// <inheritdoc />
    public double MaximumY { get; init; }

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilTinSurfaceProperties"/>
    /// </summary>
    private CivilTinSurfaceProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this TIN properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilTinSurfaceProperties Duplicate()
    {
        return new CivilTinSurfaceProperties()
        {
            Name = this.Name,
            MinimumElevation = this.MinimumElevation,
            MaximumElevation = this.MaximumElevation,
            MinimumX = this.MinimumX,
            MaximumX = this.MaximumX,
            MinimumY = this.MinimumY,
            MaximumY = this.MaximumY,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"TIN Properties: {this.Name} (Elev: {this.MinimumElevation:F2} - {this.MaximumElevation:F2})";
    }
}
