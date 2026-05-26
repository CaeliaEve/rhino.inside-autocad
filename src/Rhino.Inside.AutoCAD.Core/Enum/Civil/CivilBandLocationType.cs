namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the location of a band in a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This enumeration mirrors <c>Autodesk.Civil.DatabaseServices.BandLocationType</c>
/// from the Civil 3D API.
/// </remarks>
public enum CivilBandLocationType
{
    /// <summary>
    /// The band is located at the bottom of the profile view.
    /// </summary>
    Bottom = 0,

    /// <summary>
    /// The band is located at the top of the profile view.
    /// </summary>
    Top = 1
}
