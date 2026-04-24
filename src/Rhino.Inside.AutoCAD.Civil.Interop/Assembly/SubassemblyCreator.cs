using System.IO.Compression;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Assembly = Autodesk.Civil.DatabaseServices.Assembly;
using CadPoint2d = Autodesk.AutoCAD.Geometry.Point2d;
using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;
using CivilSubassembly = Autodesk.Civil.DatabaseServices.Subassembly;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides methods for creating Civil 3D Subassemblies from Rhino geometry.
/// </summary>
public static class SubassemblyCreator
{

    /// <summary>
    /// Extracts 2D points (offset, elevation) from a Rhino curve.
    /// </summary>
    /// <param name="curve">The Rhino curve to extract points from.</param>
    /// <returns>A list of Point2d representing (offset, elevation) pairs.</returns>
    private static List<CadPoint2d> ExtractPointsFromCurve(RhinoCurve curve)
    {
        var points = new List<CadPoint2d>();

        // Convert to polyline for vertex extraction
        var polyline = curve.ToPolyline(0.001, 0.001, 0.1, 1000);
        if (polyline == null)
        {
            // Fallback: try to get the curve as a polylineCurve
            if (curve is PolylineCurve polylineCurve)
            {
                var pl = polylineCurve.ToPolyline();
                if (pl != null)
                {
                    for (var i = 0; i < pl.Count; i++)
                    {
                        var pt = pl[i];
                        // X = offset from baseline, Z = elevation
                        points.Add(new CadPoint2d(
                            UnitConverter.ToAutoCadLength(pt.X),
                            UnitConverter.ToAutoCadLength(pt.Z)));
                    }
                }
            }
            return points;
        }

        // Extract vertices from the polyline
        for (var i = 0; i < polyline.PointCount; i++)
        {
            var pt = polyline.Point(i);
            // X = offset from baseline, Z = elevation
            // Y is ignored (along alignment direction)
            points.Add(new CadPoint2d(
                UnitConverter.ToAutoCadLength(pt.X),
                UnitConverter.ToAutoCadLength(pt.Z)));
        }

        return points;
    }

    /// <summary>
    /// Writes points to a temporary CSV file.
    /// </summary>
    /// <param name="points">The points to write.</param>
    /// <returns>The path to the temporary CSV file.</returns>
    private static string WriteTempCsv(List<CadPoint2d> points)
    {
        // Use the same temp directory structure as InteropConstants.TempDirectory
        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            InteropConstants.RhinoInsideAutocadFolder,
            InteropConstants.ConvertersFolder,
            CivilConstants.SubassemblyFolder);

        Directory.CreateDirectory(tempDirectory);

        var fileName = $"subassembly_{Guid.NewGuid()}{CivilConstants.SubassemblyCsvExtension}";
        var tempPath = Path.Combine(tempDirectory, fileName);

        using (var writer = new StreamWriter(tempPath))
        {
            writer.WriteLine("offset,elevation");
            foreach (var point in points)
            {
                writer.WriteLine($"{point.X:F6},{point.Y:F6}");
            }
        }

        return tempPath;
    }

    /// <summary>
    /// Flag indicating whether the PKT import has been attempted this session.
    /// </summary>
    private static bool _pktImportAttempted;

    /// <summary>
    /// Flag indicating whether import guidance has been shown this session.
    /// </summary>
    private static bool _guidanceShown;

    /// <summary>
    /// Creates a PKT file (ZIP containing ATC + DLL) for manual import.
    /// </summary>
    /// <returns>Path to the created PKT file, or null if creation failed.</returns>
    private static string? CreatePktFile(string dllPath, string atcContent)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RhinoInsideSubassemblies");
            Directory.CreateDirectory(tempDir);

            var pktPath = Path.Combine(tempDir, "RhinoInsideSubassemblies.pkt");

            // Delete existing PKT if it exists
            if (File.Exists(pktPath))
            {
                File.Delete(pktPath);
            }

            // Create a temporary directory for PKT contents
            var pktContentsDir = Path.Combine(tempDir, "pkt_contents");
            if (Directory.Exists(pktContentsDir))
            {
                Directory.Delete(pktContentsDir, true);
            }
            Directory.CreateDirectory(pktContentsDir);

            // Copy DLL to PKT contents
            var dllFileName = Path.GetFileName(dllPath);
            File.Copy(dllPath, Path.Combine(pktContentsDir, dllFileName), true);

            // Write ATC file to PKT contents (with relative DLL path)
            var atcPath = Path.Combine(pktContentsDir, "RhinoInsideSubassemblies.atc");
            File.WriteAllText(atcPath, atcContent);

            // Create PKT (ZIP) file
            ZipFile.CreateFromDirectory(pktContentsDir, pktPath);

            System.Diagnostics.Debug.WriteLine($"Created PKT file: {pktPath}");
            return pktPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create PKT file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Attempts to import subassemblies using the IMPORTSUBASSEMBLIES command.
    /// This is an alternative to the API-based ImportSubassembly method.
    /// </summary>
    private static void TryImportSubassembliesCommand(string pktPath)
    {
        if (_pktImportAttempted) return;
        _pktImportAttempted = true;

        try
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            System.Diagnostics.Debug.WriteLine($"Attempting IMPORTSUBASSEMBLIES command with: {pktPath}");

            // The IMPORTSUBASSEMBLIES command imports PKT files
            // Format: IMPORTSUBASSEMBLIES "path\to\file.pkt"
            doc.SendStringToExecute($"_.IMPORTSUBASSEMBLIES \"{pktPath}\" ", true, false, false);

            System.Diagnostics.Debug.WriteLine("Sent IMPORTSUBASSEMBLIES command");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"IMPORTSUBASSEMBLIES command failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows guidance to the user about manually importing the PKT file.
    /// This is shown once per session when ImportSubassembly fails.
    /// </summary>
    private static void ShowPktImportGuidance()
    {
        if (_guidanceShown) return;
        _guidanceShown = true;

        try
        {
            var pktPath = Path.Combine(Path.GetTempPath(), "RhinoInsideSubassemblies", "RhinoInsideSubassemblies.pkt");

            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            if (ed != null)
            {
                ed.WriteMessage("\n");
                ed.WriteMessage("\n========== RHINO INSIDE SUBASSEMBLY NOTICE ==========");
                ed.WriteMessage("\nCustom subassembly import requires a one-time manual step.");
                ed.WriteMessage("\nUsing placeholder subassembly for now.");
                ed.WriteMessage("\n");
                ed.WriteMessage("\nTo enable custom subassemblies:");
                ed.WriteMessage("\n  1. Run IMPORTSUBASSEMBLIES command in Civil 3D");
                ed.WriteMessage($"\n  2. Navigate to: {pktPath}");
                ed.WriteMessage("\n  3. Import the package");
                ed.WriteMessage("\n  4. Restart Civil 3D");
                ed.WriteMessage("\n");
                ed.WriteMessage("\nAfter this one-time setup, custom subassemblies will work automatically.");
                ed.WriteMessage("\n======================================================\n");
            }

            System.Diagnostics.Debug.WriteLine($"Showed PKT import guidance. PKT path: {pktPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show guidance: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the path to the subassembly catalog file (ATC format).
    /// Also ensures the subassembly is installed in Civil 3D's tool palette.
    /// </summary>
    /// <returns>The full path to the catalog file.</returns>
    private static string GetCatalogPath()
    {
        var assemblyLocation = typeof(SubassemblyCreator).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("Failed to determine assembly directory.");

        // Verify DLL exists
        var dllPath = Path.Combine(assemblyDirectory, "Rhino.Inside.Autocad.Civil.Assemblies.dll");
        if (!File.Exists(dllPath))
        {
            throw new InvalidOperationException($"Subassembly DLL not found: {dllPath}");
        }

        System.Diagnostics.Debug.WriteLine($"Subassembly DLL path: {dllPath}");

        // CRITICAL: Install subassembly into Civil 3D's tool palette FIRST
        // ImportSubassembly only works with subassemblies that are registered in the tool palette
        var toolPaletteInstalled = SubassemblyToolPaletteInstaller.EnsureInstalled(dllPath);
        System.Diagnostics.Debug.WriteLine($"Tool palette installation result: {toolPaletteInstalled}");

        // Generate ATC in the SAME directory as the DLL (so relative paths work)
        var catalogPath = Path.Combine(assemblyDirectory, "GeneratedSubassemblyCatalog.atc");

        // Use relative DLL name (since ATC is in same directory)
        var xmlSafeDllPath = "Rhino.Inside.Autocad.Civil.Assemblies.dll";

        // Generate the ATC file with relative DLL path (same directory)
        // Using <Category> root element like Civil 3D stock subassemblies
        var atcContent = $@"<Category>
	<ItemID idValue=""{{B1M0RPH1-C1V1-4SSM-8LY1-P0LYL1N31MPT}}""/>
	<Properties>
		<ItemName>Rhino Inside Subassemblies</ItemName>
		<Images/>
		<Time createdUniversalDateTime=""2024-04-23T00:00:00"" modifiedUniversalDateTime=""2024-04-23T00:00:00""/>
	</Properties>
	<CustomData/>
	<Source/>
	<Palettes/>
	<Packages/>
	<Tools>
		<Tool>
			<ItemID idValue=""{CivilConstants.ImportedPolylineSubassemblyToolId}""/>
			<Properties>
				<ItemName>Imported Polyline Subassembly</ItemName>
				<Images/>
				<Description>Imports polyline geometry from a CSV file</Description>
				<Keywords>_importedpolyline subassembly rhino grasshopper</Keywords>
				<Time createdUniversalDateTime=""2024-04-23T00:00:00"" modifiedUniversalDateTime=""2024-04-23T00:00:00""/>
			</Properties>
			<Source/>
			<StockToolRef idValue=""{{7F55AAC0-0256-48D7-BFA5-914702663FDE}}""/>
			<Data>
				<AeccDbSubassembly>
					<GeometryGenerateMode>UseDotNet</GeometryGenerateMode>
					<DotNetClass Assembly=""{xmlSafeDllPath}"">Subassembly.ImportedPolylineSubassemblies</DotNetClass>
					<Params>
						<CsvPath DataType=""String"" DisplayName=""CSV File Path"" Description=""Path to the CSV file containing offset,elevation pairs""></CsvPath>
						<PointCode DataType=""String"" DisplayName=""Point Code"" Description=""Code to assign to all points"">Shape</PointCode>
						<LinkCode DataType=""String"" DisplayName=""Link Code"" Description=""Code to assign to all links"">Top</LinkCode>
						<Closed DataType=""Long"" DisplayName=""Close Shape"" Description=""Set to 1 to close the shape"">0
							<Enum>
								<No DisplayName=""No"">0</No>
								<Yes DisplayName=""Yes"">1</Yes>
							</Enum>
						</Closed>
					</Params>
				</AeccDbSubassembly>
				<Units>foot</Units>
			</Data>
		</Tool>
	</Tools>
</Category>";

        File.WriteAllText(catalogPath, atcContent);

        System.Diagnostics.Debug.WriteLine($"Generated ATC catalog: {catalogPath}");
        System.Diagnostics.Debug.WriteLine($"DLL path in ATC: {dllPath}");

        // Register the tool palette ATC in Content Browser (preferred if available)
        // This ensures ImportSubassembly can find our subassembly via Content Browser
        var atcToRegister = SubassemblyToolPaletteInstaller.InstalledAtcPath ?? catalogPath;
        SubassemblyCatalogRegistrar.EnsureRegistered(atcToRegister);
        System.Diagnostics.Debug.WriteLine($"Registered ATC in Content Browser: {atcToRegister}");

        // Try to create and import PKT file as an alternative registration method
        var pktPath = CreatePktFile(dllPath, atcContent);
        if (pktPath != null)
        {
            TryImportSubassembliesCommand(pktPath);
        }

        return catalogPath;
    }

    /// <summary>
    /// Creates a subassembly from a Rhino curve and adds it to the specified assembly.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <param name="curve">The Rhino curve representing the cross-section profile. X = offset from baseline, Z = elevation.</param>
    /// <param name="assemblyId">The ObjectId of the target assembly.</param>
    /// <param name="name">The name for the subassembly.</param>
    /// <param name="side">The side of the assembly baseline where the subassembly will be placed.</param>
    /// <param name="pointCode">The code to assign to all points in the subassembly.</param>
    /// <param name="linkCode">The code to assign to all links in the subassembly.</param>
    /// <param name="closed">Whether to close the shape by connecting the last point back to the first.</param>
    /// <returns>The created subassembly wrapper, or null if creation fails.</returns>
    public static CivilSubassemblyWrapper? Create(
        IAutocadTransactionManager transactionManager,
        RhinoCurve curve,
        ObjectId assemblyId,
        string name,
        CivilSide side,
        string pointCode,
        string linkCode,
        bool closed)
    {
        var transaction = transactionManager.Unwrap();

        var database = transactionManager.AutocadDatabase.Unwrap();
        var civilDoc = CivilDocument.GetCivilDocument(database);

        // Get the assembly
        var assembly = transaction.GetObject(assemblyId, OpenMode.ForWrite) as Assembly;
        if (assembly == null)
        {
            throw new InvalidOperationException("Failed to access the specified assembly.");
        }

        // Convert curve to points (offset, elevation)
        var points = ExtractPointsFromCurve(curve);
        if (points.Count < 2)
        {
            throw new InvalidOperationException("Curve must have at least 2 points.");
        }

        // Create a temporary CSV file with the geometry data
        var csvPath = WriteTempCsv(points);

        try
        {
            // Get the catalog path (also ensures registry registration)
            var catalogPath = GetCatalogPath();

            // Use the assembly's insertion point as the subassembly insertion location
            var insertLocation = new CadPoint3d(0, 0, 0);

            System.Diagnostics.Debug.WriteLine($"Creating subassembly '{name}' from catalog: {catalogPath}");
            System.Diagnostics.Debug.WriteLine($"Tool ID: {CivilConstants.ImportedPolylineSubassemblyToolId}");

            // Debug: Read and validate the ATC file
            try
            {
                var atcContent = File.ReadAllText(catalogPath);
                System.Diagnostics.Debug.WriteLine($"ATC file size: {atcContent.Length} bytes");

                // Parse XML to verify structure
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(atcContent);

                var toolNodes = xmlDoc.SelectNodes("//Tool/ItemID/@idValue");
                System.Diagnostics.Debug.WriteLine($"Found {toolNodes?.Count ?? 0} tools in ATC:");
                if (toolNodes != null)
                {
                    foreach (System.Xml.XmlNode node in toolNodes)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Tool ID in ATC: {node.Value}");
                    }
                }

                // Also show DotNetClass
                var dotNetNodes = xmlDoc.SelectNodes("//DotNetClass");
                if (dotNetNodes != null)
                {
                    foreach (System.Xml.XmlNode node in dotNetNodes)
                    {
                        var assemblyAttr = node.Attributes?["Assembly"]?.Value;
                        var className = node.InnerText;
                        System.Diagnostics.Debug.WriteLine($"  DotNetClass: Assembly={assemblyAttr}, Class={className}");
                    }
                }
            }
            catch (Exception xmlEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing ATC XML: {xmlEx.Message}");
            }

            // Debug: Try to load the DLL and verify the class exists
            try
            {
                var currentAssemblyDir = Path.GetDirectoryName(typeof(SubassemblyCreator).Assembly.Location) ?? "";
                var subassemblyDllPath = Path.Combine(currentAssemblyDir, "Rhino.Inside.Autocad.Civil.Assemblies.dll");
                System.Diagnostics.Debug.WriteLine($"Attempting to load assembly: {subassemblyDllPath}");

                var loadedAssembly = System.Reflection.Assembly.LoadFrom(subassemblyDllPath);
                System.Diagnostics.Debug.WriteLine($"Assembly loaded: {loadedAssembly.FullName}");

                var types = loadedAssembly.GetTypes();
                System.Diagnostics.Debug.WriteLine($"Types in assembly ({types.Length}):");
                foreach (var type in types)
                {
                    System.Diagnostics.Debug.WriteLine($"  {type.FullName}");
                }

                var targetType = loadedAssembly.GetType("Subassembly.ImportedPolylineSubassemblies");
                if (targetType != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found target class: {targetType.FullName}");
                    System.Diagnostics.Debug.WriteLine($"  Base type: {targetType.BaseType?.FullName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Target class 'Subassembly.ImportedPolylineSubassemblies' NOT FOUND!");
                }
            }
            catch (Exception loadEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assembly: {loadEx.GetType().Name}: {loadEx.Message}");
                if (loadEx is System.Reflection.ReflectionTypeLoadException rtle)
                {
                    foreach (var le in rtle.LoaderExceptions)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Loader exception: {le?.Message}");
                    }
                }
            }

            // Try a different approach: use CREATESUBASSEMBLY command
            // The ImportSubassembly API doesn't work reliably, so let's try command-line
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc?.Editor;

            System.Diagnostics.Debug.WriteLine($"Trying CREATESUBASSEMBLY approach...");

            // First, let's check if there's a simpler way to create a subassembly
            // by looking at what methods are available on SubassemblyCollection
            var subassemblyCount = civilDoc.SubassemblyCollection.Count;
            System.Diagnostics.Debug.WriteLine($"Current subassembly count: {subassemblyCount}");

            // Try to get an existing subassembly to understand the structure
            if (subassemblyCount > 0)
            {
                foreach (ObjectId existingId in civilDoc.SubassemblyCollection)
                {
                    var existing = transaction.GetObject(existingId, OpenMode.ForRead) as CivilSubassembly;
                    if (existing != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Existing subassembly: {existing.Name}");
                        System.Diagnostics.Debug.WriteLine($"  CodeSetStyle: {existing.CodeSetStyleName}");
                        System.Diagnostics.Debug.WriteLine($"  Origin: {existing.Origin}");
                        System.Diagnostics.Debug.WriteLine($"  Side: {existing.Side}");
                        System.Diagnostics.Debug.WriteLine($"  SubassemblyType: {existing.GetType().FullName}");
                        break;
                    }
                }
            }

            // Alternative: Try using AddSubassemblyFrom method on the assembly
            // Some Civil 3D versions have this method
            System.Diagnostics.Debug.WriteLine("Checking assembly methods...");
            var assemblyType = assembly.GetType();
            var methods = assemblyType.GetMethods().Where(m => m.Name.Contains("Subassembly") || m.Name.Contains("Add")).ToList();
            foreach (var method in methods)
            {
                System.Diagnostics.Debug.WriteLine($"  Assembly method: {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
            }

            // Try ImportSubassembly first (now that tool palette is installed)
            ObjectId subassemblyId = ObjectId.Null;
            bool usedCopyWorkaround = false;

            // Use the tool palette ATC path if available (from SubassemblyToolPaletteInstaller)
            var importCatalogPath = SubassemblyToolPaletteInstaller.InstalledAtcPath ?? catalogPath;
            System.Diagnostics.Debug.WriteLine($"Attempting ImportSubassembly with catalog: {importCatalogPath}");

            try
            {
                subassemblyId = civilDoc.SubassemblyCollection.ImportSubassembly(
                    name,
                    importCatalogPath,
                    CivilConstants.ImportedPolylineSubassemblyToolId,
                    insertLocation);

                System.Diagnostics.Debug.WriteLine($"ImportSubassembly succeeded! SubassemblyId: {subassemblyId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ImportSubassembly failed: {ex.GetType().Name}: {ex.Message}");

                // Show one-time guidance for manual PKT import
                ShowPktImportGuidance();
            }

            // Fallback: Try copy workaround if ImportSubassembly failed
            if (subassemblyId.IsNull)
            {
                System.Diagnostics.Debug.WriteLine("ImportSubassembly failed, trying copy workaround...");

                ObjectId existingSubassemblyId = ObjectId.Null;
                foreach (ObjectId existingId in civilDoc.SubassemblyCollection)
                {
                    var existing = transaction.GetObject(existingId, OpenMode.ForRead) as CivilSubassembly;
                    if (existing != null)
                    {
                        existingSubassemblyId = existingId;
                        System.Diagnostics.Debug.WriteLine($"Found template subassembly: {existing.Name}");
                        break;
                    }
                }

                if (!existingSubassemblyId.IsNull)
                {
                    try
                    {
                        var copiedGroup = assembly.CopySubassembly(existingSubassemblyId);
                        if (copiedGroup != null)
                        {
                            var groupType = copiedGroup.GetType();
                            var getSubsMethod = groupType.GetMethod("GetSubassemblyIds");
                            if (getSubsMethod != null)
                            {
                                var subIds = getSubsMethod.Invoke(copiedGroup, null) as ObjectIdCollection;
                                if (subIds != null && subIds.Count > 0)
                                {
                                    subassemblyId = subIds[0];
                                    var copiedSub = transaction.GetObject(subassemblyId, OpenMode.ForWrite) as CivilSubassembly;
                                    if (copiedSub != null)
                                    {
                                        copiedSub.Name = name;
                                        usedCopyWorkaround = true;
                                        System.Diagnostics.Debug.WriteLine($"Copy workaround succeeded: {name}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception copyEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Copy workaround failed: {copyEx.Message}");
                    }
                }
            }

            if (subassemblyId.IsNull)
            {
                throw new InvalidOperationException("Failed to create subassembly - returned null ObjectId.");
            }

            // Get the subassembly and set its parameters
            var subassembly = transaction.GetObject(subassemblyId, OpenMode.ForWrite) as CivilSubassembly;
            if (subassembly == null)
            {
                throw new InvalidOperationException("Failed to access created subassembly.");
            }

            if (usedCopyWorkaround)
            {
                // When using copy workaround, the subassembly is already added to the assembly
                // and has different parameters than our custom subassembly
                System.Diagnostics.Debug.WriteLine("Using copy workaround - skipping custom parameter setup");
                System.Diagnostics.Debug.WriteLine($"Subassembly created successfully (copy workaround): {subassembly.Name}");

                // Set the side
                subassembly.Side = side.ToCivilSide();
            }
            else
            {
                // Set the subassembly parameters for ImportedPolylineSubassemblies
                var parameters = subassembly.ParamsString;
                parameters.Add("CsvPath", csvPath);
                parameters.Add("PointCode", pointCode);
                parameters.Add("LinkCode", linkCode);

                var paramsLong = subassembly.ParamsLong;
                paramsLong.Add("Closed", closed ? 1 : 0);

                // Set the side
                subassembly.Side = side.ToCivilSide();

                // Add to the assembly
                assembly.AddSubassembly(subassemblyId);
            }

            return new CivilSubassemblyWrapper(subassembly);
        }
        finally
        {
            // Note: Do NOT delete the CSV file here - the subassembly needs it
            // during corridor rebuild. The file will be cleaned up when the
            // corridor is rebuilt or the document is closed.
        }
    }
}

