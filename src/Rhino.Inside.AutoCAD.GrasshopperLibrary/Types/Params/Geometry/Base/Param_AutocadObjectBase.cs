using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadEntity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Base class for AutoCAD object parameters in Grasshopper.
/// </summary>
/// <typeparam name="TGoo">The Grasshopper Goo type that wraps the AutoCAD entity.</typeparam>
/// <typeparam name="TEntity">The AutoCAD entity type.</typeparam>
public abstract class Param_AutocadObjectBase<TGoo, TEntity> : GH_PersistentGeometryParam<TGoo>,
    IReferenceParam, IGH_PreviewObject
    where TGoo : class, IGH_GeometricGoo, IGH_AutocadReference
    where TEntity : CadEntity
{
    /// <inheritdoc />
    public override string TypeName => typeof(TGoo).Name;

    /// <inheritdoc />
    protected override TGoo InstantiateT()
    {
        try
        {
            return Activator.CreateInstance<TGoo>();
        }
        catch
        {
            return default!;
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Preview_ComputeClippingBox();

    /// <inheritdoc />
    public bool IsPreviewCapable => true;

    /// <inheritdoc />
    public bool Hidden { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_AutocadObjectBase{TGoo, TEntity}"/> class.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="nickname">The nickname of the parameter.</param>
    /// <param name="description">The description of the parameter.</param>
    /// <param name="category">The category of the parameter.</param>
    /// <param name="subcategory">The subcategory of the parameter.</param>
    protected Param_AutocadObjectBase(string name, string nickname,
        string description, string category, string subcategory)
        : base(new GH_InstanceDescription(name, nickname, description, category,
            subcategory))
    {
        this.Hidden = false;
    }

    /// <summary>
    /// Creates the filter to use for selecting objects in AutoCAD.
    /// </summary>
    /// <returns>A filter that implements <see cref="IObjectFilter"/>.</returns>
    protected abstract IObjectFilter CreateSelectionFilter();

    /// <summary>
    /// Gets the message to display when prompting for a single object.
    /// </summary>
    protected abstract string SingularPromptMessage { get; }

    /// <summary>
    /// Gets the message to display when prompting for multiple objects.
    /// </summary>
    protected abstract string PluralPromptMessage { get; }

    /// <summary>
    /// Wraps an AutoCAD entity in the appropriate Grasshopper Goo type.
    /// </summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>The wrapped entity as a Grasshopper Goo object.</returns>
    protected abstract TGoo WrapEntity(TEntity entity);

    /// <summary>
    /// Gives the opportunity to convert a support object into the desired TGoo type
    /// during selection.
    /// </summary>
    protected virtual bool ConvertSupportObject(IEntity entity, out TGoo supportedGoo)
    {
        supportedGoo = null;
        return false;
    }

    /// <inheritdoc />
    protected override GH_GetterResult Prompt_Singular(ref TGoo value)
    {
        var picker = new AutocadObjectPicker();

        var filter = this.CreateSelectionFilter();

        var selectionFilter = filter.GetSelectionFilter();

        var entity = picker.PickObject(selectionFilter, this.SingularPromptMessage);

        if (entity?.Unwrap() is TEntity typedEntity)
        {
            value = this.WrapEntity(typedEntity);

            return GH_GetterResult.success;
        }

        if (this.ConvertSupportObject(entity, out var supportedGoo))
        {
            value = supportedGoo;

            return GH_GetterResult.success;
        }

        // Out-of-process Live Link fallback for Standalone Rhino 8
        try
        {
            Rhino.Inside.AutoCAD.Core.UI.WindowHelper.ActivateAutoCad();

            var req = new Rhino.Inside.AutoCAD.Core.IPC.SelectRequestPayload
            {
                PromptMessage = this.SingularPromptMessage,
                SingleOnly = true,
                TargetType = typeof(TEntity).Name
            };

            var resp = System.Threading.Tasks.Task.Run(() => 
                Rhino.Inside.AutoCAD.Core.IPC.LiveLinkClient.Instance.RequestSelectionAsync(req, 60000)).GetAwaiter().GetResult();
            if (resp != null && resp.Success && resp.Objects.Count > 0)
            {
                var objDto = resp.Objects[0];
                var refId = !string.IsNullOrEmpty(objDto.Handle) ? new AutocadReferenceId(objDto.Handle) : AutocadReferenceId.NoReference;

                if (objDto.CurveData != null)
                {
                    var crv = Rhino.Inside.AutoCAD.Core.Converters.CadCurveReconstructor.ToRhinoCurve(objDto.CurveData);
                    if (crv != null)
                    {
                        if (typeof(TGoo) == typeof(GH_AutocadCurve))
                        {
                            value = (TGoo)(object)new GH_AutocadCurve(crv, refId);
                            this.ExpireSolution(true);
                            return GH_GetterResult.success;
                        }

                        var wrapped = CreateGooFromGeometry(crv);
                        if (wrapped is TGoo resultGoo)
                        {
                            value = resultGoo;
                            this.ExpireSolution(true);
                            return GH_GetterResult.success;
                        }
                    }
                }
                else if (objDto.Geometry3dmBytes != null && objDto.Geometry3dmBytes.Length > 0)
                {
                    using var file3dm = Rhino.FileIO.File3dm.FromByteArray(objDto.Geometry3dmBytes);
                    if (file3dm != null && file3dm.Objects.Count > 0)
                    {
                        var firstObj = System.Linq.Enumerable.FirstOrDefault(file3dm.Objects);
                        var geom = firstObj?.Geometry;
                        if (geom is Rhino.Geometry.Curve crv && typeof(TGoo) == typeof(GH_AutocadCurve))
                        {
                            value = (TGoo)(object)new GH_AutocadCurve(crv, refId);
                            this.ExpireSolution(true);
                            return GH_GetterResult.success;
                        }
                        if (geom != null)
                        {
                            var wrapped = CreateGooFromGeometry(geom);
                            if (wrapped is TGoo resultGoo)
                            {
                                value = resultGoo;
                                this.ExpireSolution(true);
                                return GH_GetterResult.success;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Rhino.RhinoApp.WriteLine($"[Rhino Live Link] Selection error: {ex.Message}");
        }

        value = default;
        return GH_GetterResult.cancel;
    }

    /// <inheritdoc />
    protected override GH_GetterResult Prompt_Plural(ref List<TGoo> values)
    {
        var picker = new AutocadObjectPicker();

        var filter = this.CreateSelectionFilter();

        var selectionFilter = filter.GetSelectionFilter();

        var entities = picker.PickObjects(selectionFilter, this.PluralPromptMessage);

        var newValues = new List<TGoo>();
        foreach (var entity in entities)
        {
            if (entity?.Unwrap() is TEntity typedEntity)
            {
                newValues.Add(this.WrapEntity(typedEntity));
            }
        }

        if (newValues.Count > 0)
        {
            values = newValues;
            this.ExpireSolution(true);
            return GH_GetterResult.success;
        }

        // Out-of-process Live Link fallback for Standalone Rhino 8
        try
        {
            Rhino.Inside.AutoCAD.Core.UI.WindowHelper.ActivateAutoCad();

            var req = new Rhino.Inside.AutoCAD.Core.IPC.SelectRequestPayload
            {
                PromptMessage = this.PluralPromptMessage,
                SingleOnly = false,
                TargetType = typeof(TEntity).Name
            };

            var resp = System.Threading.Tasks.Task.Run(() => 
                Rhino.Inside.AutoCAD.Core.IPC.LiveLinkClient.Instance.RequestSelectionAsync(req, 60000)).GetAwaiter().GetResult();
            if (resp != null && resp.Success && resp.Objects.Count > 0)
            {
                foreach (var objDto in resp.Objects)
                {
                    var refId = !string.IsNullOrEmpty(objDto.Handle) ? new AutocadReferenceId(objDto.Handle) : AutocadReferenceId.NoReference;
                    if (objDto.CurveData != null)
                    {
                        var crv = Rhino.Inside.AutoCAD.Core.Converters.CadCurveReconstructor.ToRhinoCurve(objDto.CurveData);
                        if (crv != null)
                        {
                            if (typeof(TGoo) == typeof(GH_AutocadCurve))
                            {
                                newValues.Add((TGoo)(object)new GH_AutocadCurve(crv, refId));
                                continue;
                            }

                            var wrapped = CreateGooFromGeometry(crv);
                            if (wrapped is TGoo resultGoo)
                            {
                                newValues.Add(resultGoo);
                            }
                        }
                    }
                    else if (objDto.Geometry3dmBytes != null && objDto.Geometry3dmBytes.Length > 0)
                    {
                        using var file3dm = Rhino.FileIO.File3dm.FromByteArray(objDto.Geometry3dmBytes);
                        if (file3dm != null && file3dm.Objects.Count > 0)
                        {
                            var firstObj = System.Linq.Enumerable.FirstOrDefault(file3dm.Objects);
                            if (firstObj?.Geometry is Rhino.Geometry.Curve crv && typeof(TGoo) == typeof(GH_AutocadCurve))
                            {
                                newValues.Add((TGoo)(object)new GH_AutocadCurve(crv, refId));
                                continue;
                            }
                            var geom = firstObj?.Geometry;
                            if (geom != null)
                            {
                                var wrapped = CreateGooFromGeometry(geom);
                                if (wrapped is TGoo resultGoo)
                                {
                                    newValues.Add(resultGoo);
                                }
                            }
                        }
                    }
                }

                if (newValues.Count > 0)
                {
                    values = newValues;
                    this.ExpireSolution(true);
                    return GH_GetterResult.success;
                }
            }
        }
        catch (Exception ex)
        {
            Rhino.RhinoApp.WriteLine($"[Rhino Live Link] Selection error: {ex.Message}");
        }

        values = newValues;
        return GH_GetterResult.cancel;
    }

    private IGH_Goo? CreateGooFromGeometry(Rhino.Geometry.GeometryBase geom)
    {
        if (geom is Rhino.Geometry.Curve crv)
        {
            try
            {
                if (crv is Rhino.Geometry.LineCurve lc)
                {
                    var cadLine = lc.ToAutocadLine();
                    if (cadLine is TEntity typedLine) return this.WrapEntity(typedLine);
                }
                else if (crv is Rhino.Geometry.ArcCurve ac)
                {
                    if (ac.IsCompleteCircle)
                    {
                        var cadCircle = new Rhino.Geometry.Circle(ac.Arc).ToAutocadCircle();
                        if (cadCircle is TEntity typedCircle) return this.WrapEntity(typedCircle);
                    }
                    else
                    {
                        var cadArc = ac.ToAutocadArc();
                        if (cadArc is TEntity typedArc) return this.WrapEntity(typedArc);
                    }
                }
                else if (crv is Rhino.Geometry.PolylineCurve plc)
                {
                    var cadPl = plc.ToAutocadPolyline3d();
                    if (cadPl is TEntity typedPl) return this.WrapEntity(typedPl);
                }
                else if (crv is Rhino.Geometry.PolyCurve polyCurve)
                {
                    var nurbs = polyCurve.ToNurbsCurve();
                    if (nurbs != null)
                    {
                        var cadSpline = nurbs.ToAutocadSpline();
                        if (cadSpline is TEntity typedSpline) return this.WrapEntity(typedSpline);
                    }
                }
                else if (crv is Rhino.Geometry.NurbsCurve nc)
                {
                    var cadSpline = nc.ToAutocadSpline();
                    if (cadSpline is TEntity typedSpline) return this.WrapEntity(typedSpline);
                }

                var cadCrv = crv.ToAutocadSingleCurve();
                if (cadCrv is TEntity typedEntity)
                {
                    return this.WrapEntity(typedEntity);
                }
            }
            catch (Exception ex)
            {
                Rhino.RhinoApp.WriteLine($"[Rhino Live Link] CreateGoo error: {ex.Message}");
            }
        }
        else if (geom is Rhino.Geometry.Brep brep)
        {
            return new GH_AutocadBrepProxy(brep);
        }
        else if (geom is Rhino.Geometry.Mesh mesh)
        {
            try
            {
                var cadMesh = mesh.ToAutocadSubDMesh();
                if (cadMesh is TEntity typedEntity)
                {
                    return this.WrapEntity(typedEntity);
                }
            }
            catch { }
        }
        else if (geom is Rhino.Geometry.Point pt)
        {
            try
            {
                var cadPt = pt.ToAutocadDBPoint();
                if (cadPt is TEntity typedEntity)
                {
                    return this.WrapEntity(typedEntity);
                }
            }
            catch { }
        }
        return null;
    }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change, bool includeModified = true)
    {
        foreach (var autocadId in m_data.AllData(true).OfType<TGoo>())
        {
            if (change.DoesEffectObject(autocadId.Reference.ObjectId, includeModified))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(IGH_PreviewArgs args)
    {
        if (this.Hidden) return;
        this.Preview_DrawWires(args);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(IGH_PreviewArgs args)
    {
        if (this.Hidden) return;
        this.Preview_DrawMeshes(args);
    }
}

