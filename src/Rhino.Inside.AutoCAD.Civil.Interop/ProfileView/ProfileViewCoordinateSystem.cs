using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoPlane = Rhino.Geometry.Plane;
using RhinoVector3d = Rhino.Geometry.Vector3d;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="IProfileViewCoordinateSystem"/>
public class ProfileViewCoordinateSystem : IProfileViewCoordinateSystem
{
    /// <inheritdoc />
    public RhinoPlane Plane { get; }

    /// <inheritdoc />
    public double VerticalExaggeration { get; }

    /// <inheritdoc />
    public double VerticalScale { get; }

    /// <inheritdoc />
    public double HorizontalScale { get; }

    /// <summary>
    /// Constructs a new instance of the <see cref="IProfileViewCoordinateSystem"/> class
    /// </summary>
    /// <param name="profileView"></param>
    /// <param name="transactionManager"></param>
    public ProfileViewCoordinateSystem(ProfileView profileView,
        IAutocadTransactionManager transactionManager)
    {
        var location = profileView.Location.ToRhinoPoint3d();
        var profileViewStyle = (ProfileViewStyle)transactionManager.Unwrap()
            .GetObject(profileView.StyleId, OpenMode.ForRead);

        var graphStyle = profileViewStyle.GraphStyle;

        var verticalExaggeration = graphStyle.VerticalExaggeration;

        this.VerticalScale = graphStyle.VerticalScale;
        this.VerticalExaggeration = verticalExaggeration;
        this.HorizontalScale = graphStyle.CurrentHorizontalScale;

        this.Plane = new RhinoPlane(location, RhinoVector3d.XAxis, RhinoVector3d.YAxis);
    }
}