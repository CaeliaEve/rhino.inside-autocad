using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.Windows;

namespace Rhino.Inside.AutoCAD.Applications.UI;

/// <summary>
/// Programmatic builder for the Rhino.Inside AutoCAD Ribbon Tab.
/// Guarantees that the Ribbon Tab and all buttons appear immediately on any workspace without CUIX caching issues.
/// </summary>
public static class RibbonBuilder
{
    private class RelayCommand : ICommand
    {
        private readonly Action _action;
        public RelayCommand(Action action) => _action = action;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _action();
        public event EventHandler? CanExecuteChanged;
    }

    private static bool _isHooked = false;

    /// <summary>
    /// Initializes and attaches the Ribbon tab to AutoCAD's ComponentManager with persistent document lifecycle hooks.
    /// </summary>
    public static void Initialize()
    {
        try
        {
            EnsureRibbon();

            if (!_isHooked)
            {
                _isHooked = true;
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentActivated += (s, e) => EnsureRibbon();
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.DocumentCreated += (s, e) => EnsureRibbon();
                Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle += OnApplicationIdle;
            }
        }
        catch { }
    }

    private static int _idleCheckCount = 0;
    private static void OnApplicationIdle(object? sender, EventArgs e)
    {
        EnsureRibbon();
        if (++_idleCheckCount > 10)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle -= OnApplicationIdle;
        }
    }

    /// <summary>
    /// Ensures the Ribbon tab is attached to the current active Ribbon.
    /// </summary>
    public static void EnsureRibbon()
    {
        try
        {
            if (ComponentManager.Ribbon is { } ribbon)
            {
                var tab = ribbon.Tabs.FirstOrDefault(t => t.Id == "RHINOINSIDE_TAB" || t.Title == "Rhino.Inside");
                if (tab == null)
                {
                    BuildRibbon(ribbon);
                }
                else
                {
                    tab.IsVisible = true;
                }
            }
            else
            {
                ComponentManager.ItemInitialized += OnComponentManagerItemInitialized;
            }
        }
        catch { }
    }

    private static void OnComponentManagerItemInitialized(object? sender, RibbonItemEventArgs e)
    {
        if (ComponentManager.Ribbon is { } ribbon)
        {
            ComponentManager.ItemInitialized -= OnComponentManagerItemInitialized;
            BuildRibbon(ribbon);
        }
    }

    private static BitmapImage? LoadIcon(string iconName, int size)
    {
        try
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var possiblePaths = new[]
            {
                Path.Combine(assemblyLocation ?? "", "Icons", size > 32 ? "Large512" : "Small32", iconName),
                Path.Combine(assemblyLocation ?? "", "Icons", iconName),
                Path.Combine(assemblyLocation ?? "", "..", "..", "..", "Icons", size > 32 ? "Large512" : "Small32", iconName),
                Path.Combine(assemblyLocation ?? "", "..", "..", "..", "Icons", iconName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "Rhino.Inside.AutoCAD.bundle", "Icons", size > 32 ? "Large512" : "Small32", iconName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "Rhino.Inside.AutoCAD.bundle", "Icons", iconName),
                Path.Combine(@"e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\Icons", size > 32 ? "Large512" : "Small32", iconName)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.DecodePixelWidth = size;
                    bmp.DecodePixelHeight = size;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Constructs or updates the Ribbon tab.
    /// </summary>
    public static void BuildRibbon(RibbonControl ribbon)
    {
        try
        {
            var tab = ribbon.Tabs.FirstOrDefault(t => t.Id == "RHINOINSIDE_TAB" || t.Title == "Rhino.Inside");
            if (tab == null)
            {
                tab = new RibbonTab
                {
                    Title = "Rhino.Inside",
                    Id = "RHINOINSIDE_TAB",
                    IsVisible = true
                };
                ribbon.Tabs.Add(tab);
            }

            tab.Panels.Clear();

            // Panel 1: Rhinoceros
            var rhinoPanel = new RibbonPanel();
            var rhinoSource = new RibbonPanelSource { Title = "Rhinoceros" };
            rhinoPanel.Source = rhinoSource;

            var rhinoSplit = new RibbonSplitButton
            {
                Text = "Rhino",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Rhino.png", 32),
                LargeImage = LoadIcon("Rhino.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO),
                IsSplit = true
            };

            var r7Btn = new RibbonButton
            {
                Text = "Rhino 7",
                ShowText = true,
                Image = LoadIcon("Rhino7.png", 32),
                LargeImage = LoadIcon("Rhino7.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO7)
            };

            var r8Btn = new RibbonButton
            {
                Text = "Rhino 8",
                ShowText = true,
                Image = LoadIcon("Rhino8.png", 32),
                LargeImage = LoadIcon("Rhino8.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO8)
            };

            var rSwitchBtn = new RibbonButton
            {
                Text = "Switch Version",
                ShowText = true,
                Image = LoadIcon("cog-outline.png", 32),
                LargeImage = LoadIcon("cog-outline.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.SWITCH_RHINO_VERSION)
            };

            rhinoSplit.Items.Add(r7Btn);
            rhinoSplit.Items.Add(r8Btn);
            rhinoSplit.Items.Add(rSwitchBtn);
            rhinoSource.Items.Add(rhinoSplit);

            var viewportBtn = new RibbonButton
            {
                Text = "Viewport",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("OpenViewport.png", 32),
                LargeImage = LoadIcon("OpenViewport.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.OPEN_RHINO_VIEWPORT)
            };
            rhinoSource.Items.Add(viewportBtn);

            var previewBtn = new RibbonButton
            {
                Id = "RhinoPreviewButtonId",
                Text = "Preview",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Rhinoceros_Preview_Shaded.png", 32),
                LargeImage = LoadIcon("Rhinoceros_Preview_Shaded.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.TOGGLE_RHINO_PREVIEW)
            };
            rhinoSource.Items.Add(previewBtn);

            var pkgBtn = new RibbonButton
            {
                Text = "Packages",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("PackageManager.png", 32),
                LargeImage = LoadIcon("PackageManager.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_PACKAGE_MANGER)
            };
            rhinoSource.Items.Add(pkgBtn);
            tab.Panels.Add(rhinoPanel);

            // Panel 2: Grasshopper
            var ghPanel = new RibbonPanel();
            var ghSource = new RibbonPanelSource { Title = "Grasshopper" };
            ghPanel.Source = ghSource;

            var ghSplit = new RibbonSplitButton
            {
                Text = "Grasshopper",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Grasshopper.png", 32),
                LargeImage = LoadIcon("Grasshopper.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER),
                IsSplit = true
            };

            var gh7Btn = new RibbonButton
            {
                Text = "Grasshopper 7",
                ShowText = true,
                Image = LoadIcon("Grasshopper7.png", 32),
                LargeImage = LoadIcon("Grasshopper7.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GH7)
            };

            var gh8Btn = new RibbonButton
            {
                Text = "Grasshopper 8",
                ShowText = true,
                Image = LoadIcon("Grasshopper8.png", 32),
                LargeImage = LoadIcon("Grasshopper8.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GH8)
            };

            var ghPlayerBtn = new RibbonButton
            {
                Text = "GH Player",
                ShowText = true,
                Image = LoadIcon("GrasshopperPlayer.png", 32),
                LargeImage = LoadIcon("GrasshopperPlayer.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_PLAYER)
            };

            ghSplit.Items.Add(gh7Btn);
            ghSplit.Items.Add(gh8Btn);
            ghSplit.Items.Add(ghPlayerBtn);
            ghSource.Items.Add(ghSplit);

            var previewRowPanel = new RibbonRowPanel();
            var prevOffBtn = new RibbonButton
            {
                Text = "Off",
                ShowText = false,
                Size = RibbonItemSize.Standard,
                Image = LoadIcon("Grasshopper_Preview_Off.png", 16),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_PREVIEW_OFF)
            };
            var prevWireBtn = new RibbonButton
            {
                Text = "Wire",
                ShowText = false,
                Size = RibbonItemSize.Standard,
                Image = LoadIcon("Grasshopper_Preview_Wireframe.png", 16),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_PREVIEW_WIREFRAME)
            };
            var prevShadedBtn = new RibbonButton
            {
                Text = "Shaded",
                ShowText = false,
                Size = RibbonItemSize.Standard,
                Image = LoadIcon("Grasshopper_Preview_Shaded.png", 16),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_PREVIEW_SHADED)
            };
            previewRowPanel.Items.Add(prevOffBtn);
            previewRowPanel.Items.Add(prevWireBtn);
            previewRowPanel.Items.Add(prevShadedBtn);
            ghSource.Items.Add(previewRowPanel);

            var solverBtn = new RibbonButton
            {
                Id = "GrasshopperSolverButtonId",
                Text = "Solver",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Grasshopper_SolverOn.png", 32),
                LargeImage = LoadIcon("Grasshopper_SolverOn.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_TOGGLE_SOLVER)
            };
            ghSource.Items.Add(solverBtn);

            var recomputeBtn = new RibbonButton
            {
                Text = "Recompute",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Grasshopper_Recompute.png", 32),
                LargeImage = LoadIcon("Grasshopper_Recompute.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.GRASSHOPPER_RECOMPUTE)
            };
            ghSource.Items.Add(recomputeBtn);
            tab.Panels.Add(ghPanel);

            // Panel 3: Geometry
            var geomPanel = new RibbonPanel();
            var geomSource = new RibbonPanelSource { Title = "Geometry" };
            geomPanel.Source = geomSource;

            var brepBtn = new RibbonButton
            {
                Text = "Convert BREP",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("ConvertBrep.png", 32),
                LargeImage = LoadIcon("ConvertBrep.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_INSIDE_CONVERT_BREP)
            };
            geomSource.Items.Add(brepBtn);
            tab.Panels.Add(geomPanel);

            // Panel 4: Support & Settings
            var supportPanel = new RibbonPanel();
            var supportSource = new RibbonPanelSource { Title = "Support" };
            supportPanel.Source = supportSource;

            var settingsBtn = new RibbonButton
            {
                Text = "Settings",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("cog-outline.png", 32),
                LargeImage = LoadIcon("cog-outline.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_INSIDE_SETTINGS)
            };
            supportSource.Items.Add(settingsBtn);

            var supportSplit = new RibbonSplitButton
            {
                Text = "Support",
                ShowText = true,
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                Image = LoadIcon("Bimorph.png", 32),
                LargeImage = LoadIcon("Bimorph.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_INSIDE_SUPPORT)
            };

            var aboutBtn = new RibbonButton
            {
                Text = "About",
                ShowText = true,
                Image = LoadIcon("cog-outline.png", 32),
                LargeImage = LoadIcon("cog-outline.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_INSIDE_ABOUT)
            };
            var updateBtn = new RibbonButton
            {
                Text = "Updates",
                ShowText = true,
                Image = LoadIcon("cog-outline.png", 32),
                LargeImage = LoadIcon("cog-outline.png", 64),
                CommandHandler = new RelayCommand(RhinoInsideAutoCadCommands.RHINO_INSIDE_UPDATE)
            };

            supportSplit.Items.Add(aboutBtn);
            supportSplit.Items.Add(updateBtn);
            supportSource.Items.Add(supportSplit);

            tab.Panels.Add(supportPanel);

            tab.IsActive = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Ribbon build exception: " + ex);
        }
    }
}
