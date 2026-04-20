using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilTinVolumeSurface"/>
/// <remarks>
/// Wraps an AutoCAD Civil 3D <see cref="TinVolumeSurface"/> to expose volume statistics.
/// Used by Grasshopper components to read and create volume surfaces.
/// Note: Base and comparison surface references are not exposed by the Civil 3D .NET API.
/// </remarks>
public class CivilTinVolumeSurfaceWrapper : AutocadEntityWrapper, ICivilTinVolumeSurface
{
    private readonly TinVolumeSurface _volumeSurface;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public ICivilTinVolumeSurfaceProperties VolumeProperties { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilTinVolumeSurfaceWrapper"/>.
    /// </summary>
    /// <param name="volumeSurface">
    /// The Civil 3D <see cref="TinVolumeSurface"/> to wrap.
    /// </param>
    public CivilTinVolumeSurfaceWrapper(TinVolumeSurface volumeSurface) : base(volumeSurface)
    {
        _volumeSurface = volumeSurface;
        this.Name = volumeSurface.Name;
        this.VolumeProperties = CivilTinVolumeSurfaceProperties.CreateFromVolume(volumeSurface);
    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilTinVolumeSurfaceWrapper(_volumeSurface);
    }
}
