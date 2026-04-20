using Rhino.Inside.AutoCAD.Core;
using CivilAlignmentType = Autodesk.Civil.DatabaseServices.AlignmentType;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D enumeration types
/// to their Rhino.Inside equivalents.
/// </summary>
public static class CivilEnumExtensions
{
    /// <summary>
    /// Converts a Civil 3D <see cref="CivilAlignmentType"/> to the
    /// Rhino.Inside <see cref="AlignmentType"/> equivalent.
    /// </summary>
    /// <param name="civilType">The Civil 3D alignment type to convert.</param>
    /// <returns>The corresponding Rhino.Inside alignment type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static AlignmentType ToRhinoInsideAlignmentType(this CivilAlignmentType civilType)
    {
        return (AlignmentType)civilType;
    }

    /// <summary>
    /// Converts a Rhino.Inside <see cref="AlignmentType"/> to the
    /// Civil 3D <see cref="CivilAlignmentType"/> equivalent.
    /// </summary>
    /// <param name="alignmentType">The Rhino.Inside alignment type to convert.</param>
    /// <returns>The corresponding Civil 3D alignment type.</returns>
    /// <remarks>
    /// Both enumerations share the same underlying integer values,
    /// so this is a direct cast conversion.
    /// </remarks>
    public static CivilAlignmentType ToCivilAlignmentType(this AlignmentType alignmentType)
    {
        return (CivilAlignmentType)alignmentType;
    }
}
