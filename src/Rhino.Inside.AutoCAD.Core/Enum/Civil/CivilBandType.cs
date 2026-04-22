namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of a Civil 3D ProfileView band.
/// </summary>
/// <remarks>
/// This enumeration mirrors <c>Autodesk.Civil.DatabaseServices.BandType</c>
/// from the Civil 3D API.
/// </remarks>
public enum CivilBandType
{
    /// <summary>
    /// A band displaying profile data such as elevations and stations.
    /// </summary>
    ProfileData = 0,

    /// <summary>
    /// A band displaying vertical geometry information (grades, VPIs, vertical curves).
    /// </summary>
    VerticalGeometry = 1,

    /// <summary>
    /// A band displaying horizontal geometry information (tangents, curves, spirals).
    /// </summary>
    HorizontalGeometry = 2,

    /// <summary>
    /// A band displaying superelevation data.
    /// </summary>
    Superelevation = 3,

    /// <summary>
    /// A band displaying section data.
    /// </summary>
    SectionData = 4,

    /// <summary>
    /// A band displaying pipe network data.
    /// </summary>
    PipeNetwork = 5,

    /// <summary>
    /// A band displaying pressure network data.
    /// </summary>
    PressureNetwork = 6
}
