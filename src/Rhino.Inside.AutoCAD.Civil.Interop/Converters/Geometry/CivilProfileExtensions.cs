using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D Profile types to Rhino geometry types.
/// </summary>
public static class CivilProfileExtensions
{
    /// <summary>
    /// Converts a Civil 3D Profile to a Rhino Curve (PolyCurve) in station-elevation space.
    /// </summary>
    /// <param name="profile">The Civil 3D Profile to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Curve representing the profile in 2D (X=Station, Y=Elevation).</returns>
    public static RhinoCurve? ToRhinoCurve(this Profile profile, IAutocadTransactionManager transactionManager)
    {
        var wrapper = new CivilProfileWrapper(profile);

        return wrapper.ExtractCurve(transactionManager);
    }
}

