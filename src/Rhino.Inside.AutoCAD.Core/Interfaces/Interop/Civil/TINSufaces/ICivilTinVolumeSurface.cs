namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a Civil 3D TIN Volume Surface, which is created from two TIN surfaces
/// (base and comparison) and calculates cut/fill volumes between them.
/// </summary>
/// <remarks>
/// A TIN Volume Surface inherits from TIN Surface and provides volume calculation
/// capabilities including unadjusted volumes, adjustment factors, and adjusted volumes.
/// Note: The base and comparison surface references are not exposed by the Civil 3D .NET API
/// after creation. They are only specified when creating the volume surface.
/// </remarks>
/// <seealso cref="INamedDbObject"/>
public interface ICivilTinVolumeSurface : INamedDbObject
{
    /// <summary>
    /// Gets the civil volume properties associated with this TIN Volume Surface,
    /// including cut/fill volumes.
    /// </summary>
    ICivilTinVolumeSurfaceProperties VolumeProperties { get; }
}
