using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilTinVolumeSurface"/>
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
