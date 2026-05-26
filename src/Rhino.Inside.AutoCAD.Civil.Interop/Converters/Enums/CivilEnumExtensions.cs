using Rhino.Inside.AutoCAD.Core;
using CivilAlignmentType = Autodesk.Civil.DatabaseServices.AlignmentType;
using CivilAssemblyTypeEnum = Autodesk.Civil.DatabaseServices.AssemblyType;
using CivilBandLocationTypeEnum = Autodesk.Civil.BandLocationType;
using CivilBandTypeEnum = Autodesk.Civil.BandType;
using CivilProfileTypeEnum = Autodesk.Civil.DatabaseServices.ProfileType;
using CivilSubassemblySideEnum = Autodesk.Civil.DatabaseServices.SubassemblySideType;
using CivilSurfaceBoundaryTypeEnum = Autodesk.Civil.SurfaceBoundaryType;
using CivilSurfaceBreaklineTypeEnum = Autodesk.Civil.SurfaceBreaklineType;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D enumeration types
/// to their Rhino.Inside equivalents.
/// </summary>
public static class CivilEnumExtensions
{
    /// <summary>
    /// Converts a Civil 3D <see cref="CivilAlignmentType"/> to the
    /// Rhino.Inside <see cref="Core.CivilAlignmentType"/> equivalent.
    /// </summary>
    /// <param name="civilType">The Civil 3D alignment type to convert.</param>
    /// <returns>The corresponding Rhino.Inside alignment type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static Core.CivilAlignmentType ToRhinoInsideAlignmentType(this CivilAlignmentType civilType)
    {
        return (Core.CivilAlignmentType)civilType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="Core.CivilAlignmentType"/> to the
    /// Civil 3D <see cref="CivilAlignmentType"/> equivalent.
    /// </summary>
    /// <param name="civilAlignmentType">The Rhino.Inside alignment type to convert.</param>
    /// <returns>The corresponding Civil 3D alignment type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilAlignmentType ToCivilAlignmentType(this Core.CivilAlignmentType civilAlignmentType)
    {
        return (CivilAlignmentType)civilAlignmentType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilAssemblyTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilAssemblyType"/> equivalent.
    /// </summary>
    /// <param name="civilType">The Civil 3D assembly type to convert.</param>
    /// <returns>The corresponding Rhino.Inside assembly type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilAssemblyType ToRhinoInsideAssemblyType(this CivilAssemblyTypeEnum civilType)
    {
        return (CivilAssemblyType)civilType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilAssemblyType"/> to the
    /// Civil 3D <see cref="CivilAssemblyTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="civilAssemblyType">The Rhino.Inside assembly type to convert.</param>
    /// <returns>The corresponding Civil 3D assembly type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilAssemblyTypeEnum ToCivilAssemblyType(this CivilAssemblyType civilAssemblyType)
    {
        return (CivilAssemblyTypeEnum)civilAssemblyType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilBandTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilBandType"/> equivalent.
    /// </summary>
    /// <param name="bandType">The Civil 3D band type to convert.</param>
    /// <returns>The corresponding Rhino.Inside band type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilBandType ToRhinoInsideBandType(this CivilBandTypeEnum bandType)
    {
        return (CivilBandType)bandType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilBandType"/> to the
    /// Civil 3D <see cref="CivilBandTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="bandType">The Rhino.Inside band type to convert.</param>
    /// <returns>The corresponding Civil 3D band type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilBandTypeEnum ToCivilBandType(this CivilBandType bandType)
    {
        return (CivilBandTypeEnum)bandType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilBandLocationTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilBandLocationType"/> equivalent.
    /// </summary>
    /// <param name="locationType">The Civil 3D band location type to convert.</param>
    /// <returns>The corresponding Rhino.Inside band location type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilBandLocationType ToRhinoInsideBandLocationType(this CivilBandLocationTypeEnum locationType)
    {
        return (CivilBandLocationType)locationType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilBandLocationType"/> to the
    /// Civil 3D <see cref="CivilBandLocationTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="locationType">The Rhino.Inside band location type to convert.</param>
    /// <returns>The corresponding Civil 3D band location type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilBandLocationTypeEnum ToCivilBandLocationType(this CivilBandLocationType locationType)
    {
        return (CivilBandLocationTypeEnum)locationType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilProfileTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilProfileType"/> equivalent.
    /// </summary>
    /// <param name="profileType">The Civil 3D profile type to convert.</param>
    /// <returns>The corresponding Rhino.Inside profile type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilProfileType ToRhinoInsideProfileType(this CivilProfileTypeEnum profileType)
    {
        return (CivilProfileType)profileType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilProfileType"/> to the
    /// Civil 3D <see cref="CivilProfileTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="profileType">The Rhino.Inside profile type to convert.</param>
    /// <returns>The corresponding Civil 3D profile type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilProfileTypeEnum ToCivilProfileType(this CivilProfileType profileType)
    {
        return (CivilProfileTypeEnum)profileType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilSurfaceBreaklineTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilSurfaceBreaklineType"/> equivalent.
    /// </summary>
    /// <param name="breaklineType">The Civil 3D surface breakline type to convert.</param>
    /// <returns>The corresponding Rhino.Inside surface breakline type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSurfaceBreaklineType ToRhinoInsideBreaklineType(this CivilSurfaceBreaklineTypeEnum breaklineType)
    {
        return (CivilSurfaceBreaklineType)breaklineType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilSurfaceBreaklineType"/> to the
    /// Civil 3D <see cref="CivilSurfaceBreaklineTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="breaklineType">The Rhino.Inside surface breakline type to convert.</param>
    /// <returns>The corresponding Civil 3D surface breakline type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSurfaceBreaklineTypeEnum ToCivilBreaklineType(this CivilSurfaceBreaklineType breaklineType)
    {
        return (CivilSurfaceBreaklineTypeEnum)breaklineType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilSurfaceBoundaryTypeEnum"/> to the
    /// Rhino.Inside <see cref="CivilSurfaceBoundaryType"/> equivalent.
    /// </summary>
    /// <param name="boundaryType">The Civil 3D surface boundary type to convert.</param>
    /// <returns>The corresponding Rhino.Inside surface boundary type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSurfaceBoundaryType ToRhinoInsideBoundaryType(this CivilSurfaceBoundaryTypeEnum boundaryType)
    {
        return (CivilSurfaceBoundaryType)boundaryType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilSurfaceBoundaryType"/> to the
    /// Civil 3D <see cref="CivilSurfaceBoundaryTypeEnum"/> equivalent.
    /// </summary>
    /// <param name="boundaryType">The Rhino.Inside surface boundary type to convert.</param>
    /// <returns>The corresponding Civil 3D surface boundary type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSurfaceBoundaryTypeEnum ToCivilBoundaryType(this CivilSurfaceBoundaryType boundaryType)
    {
        return (CivilSurfaceBoundaryTypeEnum)boundaryType;
    }

    /// <summary>
    /// Converts a Civil 3D <see cref="CivilSubassemblySideEnum"/> to the
    /// Rhino.Inside <see cref="CivilSide"/> equivalent.
    /// </summary>
    /// <param name="side">The Civil 3D subassembly side to convert.</param>
    /// <returns>The corresponding Rhino.Inside side type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSide ToRhinoInsideSide(this CivilSubassemblySideEnum side)
    {
        return (CivilSide)side;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="CivilSide"/> to the
    /// Civil 3D <see cref="CivilSubassemblySideEnum"/> equivalent.
    /// </summary>
    /// <param name="side">The Rhino.Inside side type to convert.</param>
    /// <returns>The corresponding Civil 3D subassembly side.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilSubassemblySideEnum ToCivilSide(this CivilSide side)
    {
        return (CivilSubassemblySideEnum)side;
    }
}
