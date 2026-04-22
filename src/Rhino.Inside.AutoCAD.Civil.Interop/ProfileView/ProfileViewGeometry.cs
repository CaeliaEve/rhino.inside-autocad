using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoTextEntity = Rhino.Geometry.TextEntity;
namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="IProfileViewGeometry"/>
public class ProfileViewGeometry : IProfileViewGeometry
{
    /// <inheritdoc />
    public List<RhinoCurve> Curves { get; } = new();

    /// <inheritdoc />s
    public List<RhinoTextEntity> TextEntities { get; } = new();
}
