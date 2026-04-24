using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivilSubassembly = Autodesk.Civil.DatabaseServices.Subassembly;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for unwrapping Civil 3D interface wrappers to their underlying API types.
/// </summary>
/// <remarks>
/// This converter enables direct access to native Civil 3D objects when the abstraction layer
/// needs to be bypassed for advanced operations or Civil 3D API interop.
/// Usage: <c>var nativeLabelGroup = myLabelGroup.Unwrap();</c>
/// </remarks>
/// <seealso cref="InteropConverter"/>
public static class CivilInteropConverter
{
    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentLabelGroup"/> to its underlying Civil 3D <see cref="LabelGroup"/>.
    /// </summary>
    /// <param name="labelGroup">The label group wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="LabelGroup"/> instance.</returns>
    public static AlignmentLabelGroup Unwrap(this CivilAlignmentLabelGroupWrapper labelGroup)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)labelGroup;

        return (AlignmentLabelGroup)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentLabelGroup"/> to its underlying Civil 3D <see cref="AlignmentLabelGroup"/>.
    /// </summary>
    /// <param name="labelGroup">The label group interface to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentLabelGroup"/> instance.</returns>
    public static AlignmentLabelGroup Unwrap(this ICivilAlignmentLabelGroup labelGroup)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)labelGroup;

        return (AlignmentLabelGroup)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilProfileLabelGroup"/> to its underlying Civil 3D <see cref="ProfileLabelGroup"/>.
    /// </summary>
    /// <param name="labelGroup">The profile label group wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="ProfileLabelGroup"/> instance.</returns>
    public static ProfileLabelGroup Unwrap(this CivilProfileLabelGroupWrapper labelGroup)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)labelGroup;

        return (ProfileLabelGroup)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilProfileLabelGroup"/> to its underlying Civil 3D <see cref="ProfileLabelGroup"/>.
    /// </summary>
    /// <param name="labelGroup">The profile label group interface to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="ProfileLabelGroup"/> instance.</returns>
    public static ProfileLabelGroup Unwrap(this ICivilProfileLabelGroup labelGroup)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)labelGroup;

        return (ProfileLabelGroup)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilFeatureLabel"/> to its underlying Civil 3D <see cref="FeatureLabel"/>.
    /// </summary>
    /// <param name="label">The feature label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="FeatureLabel"/> instance.</returns>
    public static FeatureLabel Unwrap(this ICivilFeatureLabel label)
    {
        return label switch
        {
            CivilAlignmentCurveLabelWrapper curveLabel => curveLabel.AutocadObject,
            CivilAlignmentSpiralLabelWrapper spiralLabel => spiralLabel.AutocadObject,
            CivilAlignmentTangentLabelWrapper tangentLabel => tangentLabel.AutocadObject,
            CivilAlignmentPILabelWrapper piLabel => piLabel.AutocadObject,
            CivilAlignmentIndexedPILabelWrapper indexedPiLabel => indexedPiLabel.AutocadObject,
            _ => throw new ArgumentException($"Unsupported feature label type: {label.GetType().Name}", nameof(label))
        };
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentCurveLabel"/> to its underlying Civil 3D <see cref="AlignmentCurveLabel"/>.
    /// </summary>
    /// <param name="label">The curve label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentCurveLabel"/> instance.</returns>
    public static AlignmentCurveLabel Unwrap(this CivilAlignmentCurveLabelWrapper label)
    {
        return label.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentSpiralLabel"/> to its underlying Civil 3D <see cref="AlignmentSpiralLabel"/>.
    /// </summary>
    /// <param name="label">The spiral label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentSpiralLabel"/> instance.</returns>
    public static AlignmentSpiralLabel Unwrap(this CivilAlignmentSpiralLabelWrapper label)
    {
        return label.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentTangentLabel"/> to its underlying Civil 3D <see cref="AlignmentTangentLabel"/>.
    /// </summary>
    /// <param name="label">The tangent label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentTangentLabel"/> instance.</returns>
    public static AlignmentTangentLabel Unwrap(this CivilAlignmentTangentLabelWrapper label)
    {
        return label.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentPILabel"/> to its underlying Civil 3D <see cref="AlignmentPILabel"/>.
    /// </summary>
    /// <param name="label">The PI label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentPILabel"/> instance.</returns>
    public static AlignmentPILabel Unwrap(this CivilAlignmentPILabelWrapper label)
    {
        return label.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentIndexedPILabel"/> to its underlying Civil 3D <see cref="AlignmentIndexedPILabel"/>.
    /// </summary>
    /// <param name="label">The indexed PI label wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="AlignmentIndexedPILabel"/> instance.</returns>
    public static AlignmentIndexedPILabel Unwrap(this CivilAlignmentIndexedPILabelWrapper label)
    {
        return label.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignment"/> to its underlying Civil 3D <see cref="Alignment"/>.
    /// </summary>
    /// <param name="alignment">The alignment wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="Alignment"/> instance.</returns>
    public static Alignment Unwrap(this CivilAlignmentWrapper alignment)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)alignment;

        return (Alignment)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilAlignment"/> to its underlying Civil 3D <see cref="Alignment"/>.
    /// </summary>
    /// <param name="alignment">The alignment interface to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="Alignment"/> instance.</returns>
    public static Alignment Unwrap(this ICivilAlignment alignment)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)alignment;

        return (Alignment)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps a <see cref="CivilProfileWrapper"/> to its underlying Civil 3D <see cref="Profile"/>.
    /// </summary>
    /// <param name="profile">The profile wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="Profile"/> instance.</returns>
    public static Profile Unwrap(this CivilProfileWrapper profile)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)profile;

        return (Profile)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilProfile"/> to its underlying Civil 3D <see cref="Profile"/>.
    /// </summary>
    /// <param name="profile">The profile interface to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="Profile"/> instance.</returns>
    public static Profile Unwrap(this ICivilProfile profile)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)profile;

        return (Profile)wrapper.AutocadObject;
    }

    /// <summary>
    /// Unwraps an <see cref="ICivilSubassembly"/> to its underlying Civil 3D <see cref="Subassembly"/>.
    /// </summary>
    public static CivilSubassembly Unwrap(this CivilSubassemblyWrapper subassembly)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)subassembly;

        return (CivilSubassembly)wrapper.AutocadObject;
    }
}
