using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoTextEntity = Rhino.Geometry.TextEntity;
namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="IProfileViewGeometry"/>
public class ProfileViewGeometry : IProfileViewGeometry
{
    /// <inheritdoc />
    public List<RhinoCurve> GraphCurves { get; } = new();

    /// <inheritdoc />
    public List<RhinoTextEntity> TextEntities { get; } = new();

    /// <inheritdoc />
    public List<RhinoCurve> ProfileCurves { get; } = new();

    /// <inheritdoc />
    public List<RhinoCurve> GetAllCurves()
    {
        var allCurves = new List<RhinoCurve>();
        allCurves.AddRange(this.GraphCurves);
        allCurves.AddRange(this.ProfileCurves);
        return allCurves;
    }
}
