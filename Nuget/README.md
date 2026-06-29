# NuGet packaging

This folder builds two NuGet packages that bundle the first-party
Rhino.Inside.AutoCAD assemblies so other .NET projects can
`<PackageReference>` against them. It is **independent** of the normal
`Deployment\` / WiX MSI flow — nothing here touches that pipeline.

## Packages produced

| Package id | Framework | lib folder |
|---|---|---|
| `Rhino.Inside.AutoCAD.NET48` | .NET Framework 4.8 | `lib\net48` |
| `Rhino.Inside.AutoCAD.NET8` | .NET 8 (Windows) | `lib\net8.0-windows7.0` |

Each package contains the same six first-party DLLs:

- `Rhino.Inside.AutoCAD.Core`
- `Rhino.Inside.AutoCAD.Applications`
- `Rhino.Inside.AutoCAD.Interop`
- `Rhino.Inside.AutoCAD.Services`
- `Rhino.Inside.AutoCAD.Civil.Interop`
- `Rhino.Inside.AutoCAD.UI.Resources`

Third-party libraries (Serilog, MaterialDesignThemes, System.Text.Json,
Microsoft.Extensions.*, etc.) are **declared as `<dependencies>`** in the
`.nuspec` files, not bundled, so NuGet restores them on the consumer side.
Versions differ per framework and must match `Directory.Packages.props`.

Host APIs (`RhinoCommon`, `RhinoWindows`, `Grasshopper`, `AutoCAD.NET*`,
`Civil3D.NET`) are **not** bundled or referenced — they are provided by the
AutoCAD/Rhino host at runtime.

## Prerequisites

- Windows (the build needs the AutoCAD / Rhino / Civil3D SDKs).
- .NET 8 SDK (`dotnet` on PATH).
- `nuget.exe` — if it is not on PATH, the script downloads a local copy
  (`Nuget\nuget.exe`, git-ignored) automatically on first run. To avoid the
  download, install it from https://www.nuget.org/downloads and add it to PATH.

## Usage

From anywhere (the script resolves paths relative to itself):

```bat
Nuget\build-nuget-package.bat
```

What it does:

1. Reads the version from `Directory.Build.props` (`<AssemblyVersion>`) —
   never hardcoded, so a version bump flows through automatically.
2. Builds `Release` (→ net48) and `ReleaseNET8` (→ net8.0-windows).
   It builds the `Applications` and `Civil.Interop` projects directly,
   which transitively build all six target projects. The full `.sln` is
   **not** built because it contains the WiX installer project, which
   `dotnet build` cannot process (CI removes it for the same reason).
3. Copies the six first-party DLLs (only) into `staging\`.
4. Packs both `.nuspec` files into `Output\`.

Result:

```
Nuget\Output\Rhino.Inside.AutoCAD.NET48.<version>.nupkg
Nuget\Output\Rhino.Inside.AutoCAD.NET8.<version>.nupkg
```

## Folder layout

```
Nuget\
  build-nuget-package.bat            <- the script (committed)
  Rhino.Inside.AutoCAD.NET48.nuspec  <- manifest (committed)
  Rhino.Inside.AutoCAD.NET8.nuspec   <- manifest (committed)
  staging\                           <- intermediate, recreated each run (git-ignored)
  Output\                            <- final .nupkg files (git-ignored)
```

## Maintenance

- **Version** comes from `Directory.Build.props`; no edits needed here on a bump.
- **Dependency versions** in the two `.nuspec` files are maintained by hand.
  If you change a third-party package version in `Directory.Packages.props`,
  update the matching `<dependency>` in the relevant `.nuspec`. Host APIs are
  intentionally excluded.
- **Adding/removing a bundled assembly**: edit the `PROJECTS` list in
  `build-nuget-package.bat`. (All six output DLL names equal their project
  names; no project sets an explicit `AssemblyName`.)
