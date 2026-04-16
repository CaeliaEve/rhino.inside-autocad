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
    /// Gets the raw cut volume before applying the cut factor (in cubic units).
    /// </summary>
    double UnadjustedCutVolume { get; }

    /// <summary>
    /// Gets the raw fill volume before applying the fill factor (in cubic units).
    /// </summary>
    double UnadjustedFillVolume { get; }

    /// <summary>
    /// Gets the raw net volume (unadjusted cut - unadjusted fill) before applying factors.
    /// </summary>
    double UnadjustedNetVolume { get; }

    /// <summary>
    /// Gets the cut volume adjustment factor.
    /// </summary>
    double CutFactor { get; }

    /// <summary>
    /// Gets the fill volume adjustment factor.
    /// </summary>
    double FillFactor { get; }

    /// <summary>
    /// Gets the adjusted cut volume (raw cut volume * cut factor).
    /// </summary>
    double AdjustedCutVolume { get; }

    /// <summary>
    /// Gets the adjusted fill volume (raw fill volume * fill factor).
    /// </summary>
    double AdjustedFillVolume { get; }

    /// <summary>
    /// Gets the adjusted net volume (adjusted cut - adjusted fill).
    /// </summary>
    double AdjustedNetVolume { get; }
}
