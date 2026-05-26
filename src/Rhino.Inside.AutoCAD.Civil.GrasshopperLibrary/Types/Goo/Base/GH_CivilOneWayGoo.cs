using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Base class for Civil 3D Goo types that support one-way conversion (AutoCAD to Rhino only).
/// Provides preview capabilities in both Rhino and AutoCAD viewports but does NOT support:
/// - Baking (no IAutocadBakeable interface)
/// - Transform/Morph operations (requires reverse conversion)
/// </summary>
/// <typeparam name="TWrapperType">The Civil 3D entity type (e.g., Alignment, Corridor).</typeparam>
/// <typeparam name="TRhinoType">The Rhino adapter type for preview geometry.</typeparam>
/// <remarks>
/// <para>
/// <b>ARCHITECTURAL NOTE - SUBOPTIMAL SOLUTION:</b>
/// This class implements IGH_GeometricGoo (via GH_GeometricGoo base) to satisfy the
/// constraint on Param_AutocadObjectBase. However, Transform and Morph operations
/// return the object unchanged because Civil 3D entities cannot be converted back
/// from transformed Rhino geometry.
/// </para>
/// <para>
/// A better long-term solution would be to create a separate param base class
/// (e.g., Param_CivilOneWayObjectBase) that doesn't require IGH_GeometricGoo and
/// manually implements preview via IGH_PreviewData interface.
/// </para>
/// </remarks>
public abstract class GH_CivilOneWayGoo<TWrapperType, TRhinoType>
    : GH_GeometricGoo<TWrapperType>,
      IGH_AutocadReferenceDatabaseObject,
      IGH_PreviewData,
      IGH_AutocadGeometryPreview
    where TWrapperType : Entity
    where TRhinoType : class, IRhinoAdapter
{
    private const string ReferenceHandleDictionaryName = "AutocadReferenceHandle";

    private TRhinoType? _cachedRhinoGeometry;
    private TWrapperType? _cachedValueForGeometry;

    /// <inheritdoc />
    public IAutocadReferenceId Reference { get; private set; }

    /// <inheritdoc />
    public IDbObject ObjectValue => new AutocadEntityWrapper(this.Value);

    /// <summary>
    /// Gets the Rhino geometry equivalent of the AutoCAD geometry.
    /// The result is cached and automatically invalidated when Value changes.
    /// </summary>
    public TRhinoType? RhinoGeometry
    {
        get
        {
            if (this.Value == null)
            {
                _cachedRhinoGeometry = null;
                _cachedValueForGeometry = null;
                return null;
            }

            // Recompute if Value reference has changed
            if (!ReferenceEquals(_cachedValueForGeometry, this.Value))
            {
                _cachedRhinoGeometry = this.ConvertToRhino(this.Value);
                _cachedValueForGeometry = this.Value;
            }

            return _cachedRhinoGeometry;
        }
    }

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (this.Value == null || !this.Value.Bounds.HasValue)
                return BoundingBox.Empty;

            var bounds = this.Value.Bounds;
            return bounds!.Value.ToRhinoBoundingBox();
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Boundingbox;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return $"No internal {this.TypeName} data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value != null;

    /// <inheritdoc />
    public override string TypeName => $"Civil3D {typeof(TWrapperType).Name}";

    /// <inheritdoc />
    public override string TypeDescription => $"Represents a {this.TypeName}";

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilOneWayGoo{TWrapperType, TRhinoType}"/> class with no value.
    /// </summary>
    protected GH_CivilOneWayGoo()
    {
        this.Reference = AutocadReferenceId.NoReference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilOneWayGoo{TWrapperType, TRhinoType}"/> class with the
    /// specified Civil 3D entity.
    /// </summary>
    /// <param name="entity">The Civil 3D entity to wrap.</param>
    protected GH_CivilOneWayGoo(TWrapperType? entity) : base(entity?.Clone() as TWrapperType)
    {
        this.Reference = entity is not null ? new AutocadReferenceId(entity) : AutocadReferenceId.NoReference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilOneWayGoo{TWrapperType, TRhinoType}"/> class with the
    /// specified Civil 3D entity and reference ID.
    /// </summary>
    /// <param name="entity">The Civil 3D entity to wrap.</param>
    /// <param name="referenceId">The AutoCAD ObjectId to bind to this reference.</param>
    protected GH_CivilOneWayGoo(TWrapperType entity, IAutocadReferenceId referenceId) : base(entity)
    {
        this.Reference = referenceId;
    }

    /// <summary>
    /// Creates a new instance wrapping the specified entity. The internal entity should be cloned,
    /// but the reference ID must be preserved.
    /// </summary>
    protected abstract GH_CivilOneWayGoo<TWrapperType, TRhinoType> CreateClonedInstance(TWrapperType entity);

    /// <summary>
    /// Creates a new instance wrapping the specified entity. The internal entity is cloned
    /// and the reference ID is reset.
    /// </summary>
    protected abstract GH_CivilOneWayGoo<TWrapperType, TRhinoType> CreateInstance(TWrapperType entity);

    /// <summary>
    /// Converts the Civil 3D entity to its Rhino representation for preview purposes.
    /// </summary>
    /// <param name="wrapperType">The Civil 3D entity to convert.</param>
    /// <returns>The Rhino adapter containing preview geometry, or null if conversion fails.</returns>
    protected abstract TRhinoType? ConvertToRhino(TWrapperType wrapperType);

    /// <summary>
    /// Draws the geometry in the Rhino viewport for wire views.
    /// </summary>
    protected abstract void DrawViewportGeometryWires(GH_PreviewWireArgs args);

    /// <summary>
    /// Draws the geometry in the Rhino viewport for mesh views.
    /// </summary>
    protected abstract void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args);

    /// <inheritdoc />
    public abstract void DrawAutocadPreview(IGrasshopperPreviewData previewData);

    /// <inheritdoc />
    public override IGH_Goo Duplicate() => this.CreateClonedInstance(this.Value);

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry() =>
        (IGH_GeometricGoo)this.CreateClonedInstance(this.Value);

    /// <inheritdoc />
    public override BoundingBox GetBoundingBox(Transform xform)
    {
        var box = this.Boundingbox;
        box.Transform(xform);
        return box;
    }

    /// <summary>
    /// Returns this instance unchanged. Transform is not supported for one-way Civil 3D types.
    /// </summary>
    /// <remarks>
    /// Civil 3D entities (Alignments, Corridors, etc.) cannot be recreated from transformed
    /// Rhino geometry. This method exists only to satisfy IGH_GeometricGoo interface requirements.
    /// </remarks>
    public override IGH_GeometricGoo Transform(Transform xform) => this;

    /// <summary>
    /// Returns this instance unchanged. Morph is not supported for one-way Civil 3D types.
    /// </summary>
    /// <remarks>
    /// Civil 3D entities (Alignments, Corridors, etc.) cannot be recreated from morphed
    /// Rhino geometry. This method exists only to satisfy IGH_GeometricGoo interface requirements.
    /// </remarks>
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) => this;

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (this.RhinoGeometry == null)
            return;

        this.DrawViewportGeometryWires(args);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        if (this.RhinoGeometry == null)
            return;

        this.DrawViewportGeometryMeshes(args);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilOneWayGoo<TWrapperType, TRhinoType> goo)
        {
            this.Value = goo.Value;
            return true;
        }

        if (source is TWrapperType wrapperType)
        {
            this.Value = wrapperType;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(TWrapperType)))
        {
            target = (Q)(object)this.Value;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilOneWayGoo<TWrapperType, TRhinoType>)))
        {
            target = (Q)(object)this.CreateInstance(this.Value);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetUpdatedObject()
    {
        var picker = new AutocadObjectPicker();
        if (picker.TryGetUpdatedObject(this.Reference.ObjectId, out var entity))
        {
            this.Value = (TWrapperType?)entity.Unwrap();
        }
    }

    /// <inheritdoc />
    public override bool Read(GH_IReader reader)
    {
        var referenceHandle = string.Empty;

        reader.TryGetString(ReferenceHandleDictionaryName, ref referenceHandle);

        if (string.IsNullOrEmpty(referenceHandle))
            return true;

        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        var database = activeDocument.Database;

        var handle = new Handle(System.Convert.ToInt64(referenceHandle, 16));

        var transaction = database.TransactionManager.StartTransaction();

        var newId = database.GetObjectId(false, handle, 0);

        if (newId.IsValid == false) return true;

        var referencedObject = transaction.GetObject(newId, OpenMode.ForRead);

        if (referencedObject is TWrapperType typeReferencedObject == false)
            return true;

        this.Value = typeReferencedObject;

        this.Reference = new AutocadReferenceId(typeReferencedObject);

        transaction.Commit();

        return true;
    }

    /// <inheritdoc />
    public override bool Write(GH_IWriter writer)
    {
        if (this.Reference.IsValid && this.Value is not null)
            writer.SetString(ReferenceHandleDictionaryName, this.Reference.GetSerializedValue());

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return $"Null {this.TypeName}";

        return $"{this.TypeName} [Type: {this.Value.GetType().Name}, Id: {this.Reference}]";
    }
}
