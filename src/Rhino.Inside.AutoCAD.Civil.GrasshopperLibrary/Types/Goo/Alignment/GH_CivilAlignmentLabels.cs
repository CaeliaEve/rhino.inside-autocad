using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using CivilFeatureLabel = Autodesk.Civil.DatabaseServices.FeatureLabel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Grasshopper Goo for Civil 3D Alignment labels (generic - can hold any label type).
/// </summary>
public class GH_CivilFeatureLabel : GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter>
{
    protected override GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter> CreateClonedInstance(CivilFeatureLabel entity)
    {
        throw new NotImplementedException();
    }

    protected override GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter> CreateInstance(CivilFeatureLabel entity)
    {
        throw new NotImplementedException();
    }

    protected override CivilFeatureLabel? Convert(FeatureLabelAdapter rhinoType)
    {
        throw new NotImplementedException();
    }

    protected override FeatureLabelAdapter? Convert(CivilFeatureLabel wrapperType)
    {
        throw new NotImplementedException();
    }

    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        throw new NotImplementedException();
    }

    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        throw new NotImplementedException();
    }

    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        throw new NotImplementedException();
    }
}
