using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// The volume statistics extracted from a Civil 3D TIN Volume Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted volume property information.
/// The data is captured at construction time from a <see cref="TinVolumeSurface"/>.
/// </remarks>
public record CivilTinVolumeSurfaceProperties : ICivilTinVolumeSurfaceProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilTinVolumeSurfaceProperties"/> by extracting
    /// data from a given <see cref="TinVolumeSurface"/>.
    /// </summary>
    public static CivilTinVolumeSurfaceProperties CreateFromVolume(TinVolumeSurface volumeSurface)
    {
        var props = volumeSurface.GetVolumeProperties();

        return new CivilTinVolumeSurfaceProperties()
        {
            UnadjustedCutVolume = props.UnadjustedCutVolume,
            UnadjustedFillVolume = props.UnadjustedFillVolume,
            UnadjustedNetVolume = props.UnadjustedCutVolume - props.UnadjustedFillVolume,
            CutFactor = props.CutFactor,
            FillFactor = props.FillFactor,
            AdjustedCutVolume = props.AdjustedCutVolume,
            AdjustedFillVolume = props.AdjustedFillVolume,
            AdjustedNetVolume = props.AdjustedCutVolume - props.AdjustedFillVolume,
            Style = new NamedId(volumeSurface.StyleName, volumeSurface.StyleId),
        };
    }

    /// <inheritdoc />
    public double UnadjustedCutVolume { get; private set; }

    /// <inheritdoc />
    public double UnadjustedFillVolume { get; private set; }

    /// <inheritdoc />
    public double UnadjustedNetVolume { get; private set; }

    /// <inheritdoc />
    public double CutFactor { get; private set; }

    /// <inheritdoc />
    public double FillFactor { get; private set; }

    /// <inheritdoc />
    public double AdjustedCutVolume { get; private set; }

    /// <inheritdoc />
    public double AdjustedFillVolume { get; private set; }

    /// <inheritdoc />
    public double AdjustedNetVolume { get; private set; }

    /// <inheritdoc />
    public INamedId Style { get; private set; } = NamedId.Empty;

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilTinVolumeSurfaceProperties"/>
    /// </summary>
    private CivilTinVolumeSurfaceProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this volume properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilTinVolumeSurfaceProperties Duplicate()
    {
        return new CivilTinVolumeSurfaceProperties()
        {
            UnadjustedCutVolume = this.UnadjustedCutVolume,
            UnadjustedFillVolume = this.UnadjustedFillVolume,
            UnadjustedNetVolume = this.UnadjustedNetVolume,
            CutFactor = this.CutFactor,
            FillFactor = this.FillFactor,
            AdjustedCutVolume = this.AdjustedCutVolume,
            AdjustedFillVolume = this.AdjustedFillVolume,
            AdjustedNetVolume = this.AdjustedNetVolume,
            Style = this.Style.ShallowClone() as NamedId ?? NamedId.Empty,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Volume Properties: Cut={this.AdjustedCutVolume:F2}, Fill={this.AdjustedFillVolume:F2}, Net={this.AdjustedNetVolume:F2}";
    }
}
