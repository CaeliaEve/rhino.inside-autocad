using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilCorridorWrapper"/>
public class CivilCorridorWrapper : AutocadEntityWrapper, ICivilCorridorWrapper
{
    private readonly Corridor _corridor;

    /// <inheritdoc />
    public ICivilCorridorProperties Properties { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="CivilCorridorWrapper"/> by wrapping a Civil
    /// 3D Corridor object.
    /// </summary>
    public CivilCorridorWrapper(Corridor corridor) : base(corridor)
    {
        _corridor = corridor;
        this.Properties = new CivilCorridorProperties(corridor);

    }

    /// <summary>
    /// Creates a shallow copy of the current <see cref="CivilCorridorWrapper"/> instance.
    /// </summary>
    /// <returns>A new <see cref="CivilCorridorWrapper"/> instance that references the same underlying corridor as the current
    /// instance.</returns>
    public override IDbObject ShallowClone()
    {
        return new CivilCorridorWrapper(_corridor);
    }

    /// <inheritdoc />
    public List<ICivilCorridorBaseline> GetBaselines(IAutocadTransactionManager transactionManager)
    {
        var baselines = new List<ICivilCorridorBaseline>();

        try
        {
            foreach (var baseline in _corridor.Baselines)
            {
                var wrapper = new CivilCorridorBaselineWrapper(baseline);
                baselines.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if baseline extraction fails
        }

        return baselines;
    }

    /// <inheritdoc />
    public List<ICivilCorridorSurface> GetCorridorSurfaces(IAutocadTransactionManager transactionManager)
    {
        var surfaces = new List<ICivilCorridorSurface>();

        try
        {
            var transaction = transactionManager.Unwrap();

            foreach (var surface in _corridor.CorridorSurfaces)
            {
                RhinoMesh? mesh = null;

                // Try to get the actual TIN surface and convert to mesh
                if (!surface.SurfaceId.IsNull && !surface.SurfaceId.IsErased)
                {
                    try
                    {
                        var tinSurface = transaction.GetObject(surface.SurfaceId, OpenMode.ForRead) as TinSurface;
                        if (tinSurface != null)
                        {
                            mesh = tinSurface.ToRhinoMesh(transactionManager);
                        }
                    }
                    catch
                    {
                        // Mesh extraction failed, wrapper will have null mesh
                    }
                }

                var wrapper = new CivilCorridorSurfaceWrapper(surface, mesh);
                surfaces.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if surface extraction fails
        }

        return surfaces;
    }
}