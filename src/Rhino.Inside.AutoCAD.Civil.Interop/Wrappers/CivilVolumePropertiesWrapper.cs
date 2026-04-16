using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps volume statistics extracted from a Civil 3D TIN Volume Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted volume property information.
/// The data is captured at construction time from a <see cref="TinVolumeSurface"/>.
/// </remarks>
public class CivilVolumePropertiesWrapper : ICivilVolumeProperties
{
    /// <inheritdoc />
    public double UnadjustedCutVolume { get; }

    /// <inheritdoc />
    public double UnadjustedFillVolume { get; }

    /// <inheritdoc />
    public double UnadjustedNetVolume { get; }

    /// <inheritdoc />
    public double CutFactor { get; }

    /// <inheritdoc />
    public double FillFactor { get; }

    /// <inheritdoc />
    public double AdjustedCutVolume { get; }

    /// <inheritdoc />
    public double AdjustedFillVolume { get; }

    /// <inheritdoc />
    public double AdjustedNetVolume { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilVolumePropertiesWrapper"/>
    /// from a Civil 3D volume surface.
    /// </summary>
    /// <param name="volumeSurface">The volume surface to extract properties from.</param>
    public CivilVolumePropertiesWrapper(TinVolumeSurface volumeSurface)
    {
        var props = volumeSurface.GetVolumeProperties();

        UnadjustedCutVolume = props.UnadjustedCutVolume;
        UnadjustedFillVolume = props.UnadjustedFillVolume;
        UnadjustedNetVolume = props.UnadjustedCutVolume - props.UnadjustedFillVolume;
        CutFactor = props.CutFactor;
        FillFactor = props.FillFactor;
        AdjustedCutVolume = props.AdjustedCutVolume;
        AdjustedFillVolume = props.AdjustedFillVolume;
        AdjustedNetVolume = props.AdjustedCutVolume - props.AdjustedFillVolume;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilVolumePropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilVolumePropertiesWrapper(
        double unadjustedCutVolume,
        double unadjustedFillVolume,
        double cutFactor,
        double fillFactor,
        double adjustedCutVolume,
        double adjustedFillVolume)
    {
        UnadjustedCutVolume = unadjustedCutVolume;
        UnadjustedFillVolume = unadjustedFillVolume;
        UnadjustedNetVolume = unadjustedCutVolume - unadjustedFillVolume;
        CutFactor = cutFactor;
        FillFactor = fillFactor;
        AdjustedCutVolume = adjustedCutVolume;
        AdjustedFillVolume = adjustedFillVolume;
        AdjustedNetVolume = adjustedCutVolume - adjustedFillVolume;
    }

    /// <summary>
    /// Creates a duplicate of this volume properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilVolumePropertiesWrapper Duplicate()
    {
        return new CivilVolumePropertiesWrapper(
            UnadjustedCutVolume,
            UnadjustedFillVolume,
            CutFactor,
            FillFactor,
            AdjustedCutVolume,
            AdjustedFillVolume);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Volume Properties: Cut={AdjustedCutVolume:F2}, Fill={AdjustedFillVolume:F2}, Net={AdjustedNetVolume:F2}";
    }
}
