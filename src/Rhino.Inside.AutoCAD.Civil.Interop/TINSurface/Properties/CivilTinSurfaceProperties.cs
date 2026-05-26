using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

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
    private readonly TinSurface _tinSurface;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ICivilSurfacePoint MinimumPoint { get; }

    /// <inheritdoc />
    public ICivilSurfacePoint MaximumPoint { get; }

    /// <inheritdoc />
    public INamedId Style { get; } = NamedId.Empty;

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilTinSurfaceProperties"/>
    /// </summary>
    public CivilTinSurfaceProperties(TinSurface tinSurface)
    {
        _tinSurface = tinSurface;
        var generalProps = tinSurface.GetGeneralProperties();

        this.Name = tinSurface.Name;

        this.MinimumPoint = new CivilSurfacePoint(generalProps.MinimumCoordinateX,
            generalProps.MinimumCoordinateY, generalProps.MinimumElevation);

        this.MaximumPoint = new CivilSurfacePoint(generalProps.MaximumCoordinateX,
            generalProps.MaximumCoordinateY, generalProps.MaximumElevation);

        this.Style = new NamedId(tinSurface.StyleName, tinSurface.StyleId);

    }

    /// <summary>
    /// Creates a duplicate of this TIN properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilTinSurfaceProperties Duplicate()
    {
        return new CivilTinSurfaceProperties(_tinSurface);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"TIN Properties: {this.Name} (Elev: {this.MinimumPoint.Elevation:F2} - {this.MaximumPoint.Elevation:F2})";
    }
}
