# LAN Zoo licensing fails on .NET 8 hosts (AutoCAD 2025/2026) — root cause and fix

**Status:** root-caused and fixed; verified working in AutoCAD 2025 on 2026-07-07.
The fix lives as uncommitted changes on the `Civil_Icons_r1` working tree (diff summary at the bottom).

## Symptom

Starting Rhino.Inside in AutoCAD 2025/2026 (NET8 builds) with a LAN Zoo license fails with:

> *There was a problem creating a communication channel with "<zoo server>". Please contact your system administrator for assistance.*

The same machine/license works fine in AutoCAD 2024 (net48 builds) and in standalone Rhino 8.

## Root cause (three layers)

1. **WCF is not part of the .NET 8 runtime.** `ZooClient.dll` (a net48-era assembly shipped with
   Rhino) talks to the LAN Zoo over WCF (`BasicHttpBinding` + `ChannelFactory<IZooClientService>`,
   see `ClientChannel.Create` in the Rhino source). On net48 the WCF assemblies come from the GAC;
   on .NET 8 they must be supplied by someone. `ClientChannel.Create` catches *any* exception and
   replaces it with the generic "communication channel" message (the real exception only goes to
   `Debug.WriteLine`, which is compiled out of release ZooClient), so the failures below are
   invisible without instrumentation.

2. **AutoCAD ships a partial WCF set and pre-loads it.** AutoCAD 2025 ships
   `System.ServiceModel.Primitives` **6.0** (plus NetNamedPipe/NetFramingBase; **no**
   `System.ServiceModel` facade, **no** `System.ServiceModel.Http`) and loads Primitives at startup
   for its own IPC. AutoCAD 2026 ships the same subset at **8.1.2**. An already-loaded assembly
   wins every subsequent bind for that simple name, so the host's Primitives version is
   non-negotiable.

3. **Rhino's in-process prober injects an incompatible family.** When RhinoCore runs in-process,
   a hook on `AssemblyLoadContext.Default.Resolving` serves assemblies from
   `Rhino 8\System\netcore`, which contains the WCF **4.9** compat family
   (`System.ServiceModel` facade, `Http` 4.9, `Private.ServiceModel` 4.9). `Default.Resolving`
   runs **before** `AppDomain.AssemblyResolve`, so plugin-registered resolvers never get asked.
   The result at Zoo time: facade + Http 4.9 + Private 4.9 (from Rhino) mixed with Primitives 6.0
   (from AutoCAD). WCF type identities split — `HttpTransportBindingElement` derives from the
   4.9 world's `TransportBindingElement`, while `ChannelFactory`'s invariant check runs in the
   6.0 world — and channel creation throws:

   > `System.InvalidOperationException: The CustomBinding on the ServiceEndpoint with contract
   > 'IZooClientService' lacks a TransportBindingElement.`

   (Captured live in AutoCAD 2025 via a temporary `FirstChanceException` logger; also reproduced
   and fix-verified in a standalone .NET 8 harness that mimics AutoCAD's loading topology.)

## Why the Rhino.Inside.Revit trick isn't enough here

RiR's rule — *resolve from `System\netcore` then `System`, but never serve `System.*` from Rhino;
let the host resolve those* (`AssemblyResolver.cs` L173/L497) — works in Revit because Revit ships
a complete, internally consistent WCF set. AutoCAD ships only *part* of a family, so "let the host
solve it" leaves the facade and the HTTP transport unresolvable, and any fallback to Rhino's copies
recreates the mixed-family split. The extended principle that works on AutoCAD:
**complete the host's WCF family with version-matched copies, and pre-load them.**

## The fix

Ship the missing WCF pieces with the plugin, version-matched to the host, and **pre-load** them at
plugin initialization (pre-loading is essential — it is the only resolution step that outranks
Rhino's netcore prober):

- `src/Rhino.Inside.AutoCAD.Interop/WCF/` (new):
  - `System.ServiceModel.dll` — the 4.9 compat facade (type-forwards only, no code; forwards by
    simple name so it lands on whatever Primitives/Http versions are loaded). Same file Rhino
    ships in `System\netcore`; originally from the `System.ServiceModel.Primitives` 4.9 NuGet.
  - `System.ServiceModel.Http.6.0.dll` / `System.ServiceModel.Primitives.6.0.dll` — NuGet 6.0.0,
    for AutoCAD 2025.
  - `System.ServiceModel.Http.8.1.dll` / `System.ServiceModel.Primitives.8.1.dll` — NuGet 8.1.2,
    for AutoCAD 2026.
  - Files carry a version suffix so both families can sit flat in the bundle folder, and so
    LoadFrom directory probing can never pick the wrong one by file name.

- `RhinoCoreExtension` (NET8 branch of the static ctor) calls new `LoadWcfAssemblies()`:
  1. Read the version of `System.ServiceModel.Primitives.dll` sitting next to `acad.exe`
     → family `"6.0"` (major ≤ 6) or `"8.1"`.
  2. `Assembly.LoadFrom` the facade and the family-matched `Http` from the plugin folder.
  3. If the host ships no Primitives at all (future-proofing), also load the plugin's copy.
  All wrapped in try/catch so licensing degrades to the old behavior rather than breaking load.

- `Rhino.Inside.AutoCAD.Interop.csproj`: copies the `WCF\*` DLLs to the output root
  (`Link` flattened) in `DebugNET8`/`ReleaseNET8` only. net48 builds (AutoCAD 2024) are untouched.

Resulting load state (verified in the diagnostic log, AutoCAD 2025):
facade 4.0.0.0 + `Http` 6.0 from the plugin bundle + Primitives 6.0 from AutoCAD — one consistent
family; ZooClient's channel builds and the license is served.

## Verification done

- AutoCAD 2025 + local Zoo 8 server: license acquired, Rhino/Grasshopper start normally. ✔
- Standalone .NET 8 harness reproducing AutoCAD's topology (host-preloaded Primitives 6.0 +
  simulated Rhino netcore prober): fails identically without the fix, succeeds with it, for both
  the 6.0 (2025) and 8.1.2 (2026) worlds. ✔
- AutoCAD 2026 / Civil 3D 2026 live test: **not yet run** (simulation-validated only).

## Notes for whoever lands this

- `src/Rhino.Inside.AutoCAD.Interop/Rhino/ZooLicenseDiagnostics.cs` is a temporary
  `FirstChanceException`/assembly-load logger (NET8-only, writes
  `%TEMP%\RhinoInside.AutoCAD.ZooDiag.log`) used to find this. Remove it, or gate it behind an
  environment variable, before release. `RhinoCoreExtension` calls `ZooLicenseDiagnostics.Install()`
  at the top of the NET8 static-ctor branch.
- The WCF NuGet assemblies are MIT-licensed (dotnet/wcf) and redistributable.
- If a future AutoCAD bumps its WCF major again, add the matching `Http`/`Primitives` pair and
  extend the family probe in `LoadWcfAssemblies`.
- Unrelated but discovered along the way: the per-project `PostBuild` xcopy into
  `%APPDATA%\Autodesk\ApplicationPlugins\...` **silently no-ops while AutoCAD is running** (locked
  files, xcopy still exits 0), which makes stale-DLL testing very easy to hit. Worth a guard or at
  least a build warning.

## Changed files

```
 M src/Rhino.Inside.AutoCAD.Interop/Rhino.Inside.AutoCAD.Interop.csproj   (+11)
 M src/Rhino.Inside.AutoCAD.Interop/Rhino/RhinoCoreExtension.cs           (+49)
 ?? src/Rhino.Inside.AutoCAD.Interop/Rhino/ZooLicenseDiagnostics.cs       (temporary diagnostics)
 ?? src/Rhino.Inside.AutoCAD.Interop/WCF/                                 (5 DLLs, see above)
```
