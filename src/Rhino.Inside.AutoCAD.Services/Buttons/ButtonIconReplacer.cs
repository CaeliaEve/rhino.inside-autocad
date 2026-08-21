using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Windows;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IButtonIconReplacer"/>
public class ButtonIconReplacer : IButtonIconReplacer
{
    private const string _rhinoInsideTabName = ApplicationConstants.RhinoInsideTabName;
    private const int _smallIconSize = ApplicationConstants.SmallIconSize;
    private const int _largeIconSize = ApplicationConstants.LargeIconSize;

    /// <inheritdoc />
    public string ButtonId { get; }

    /// <summary>
    /// Constructs a new <see cref="IButtonIconReplacer"/>
    /// </summary>
    public ButtonIconReplacer(string buttonId)
    {
        this.ButtonId = buttonId;
    }

    /// <summary>
    /// Creates a resized <see cref="BitmapImage"/> from the image safely.
    /// </summary>
    private BitmapImage? ResizeImage(string imagePath, int width, int height)
    {
        try
        {
            var fileName = Path.GetFileName(imagePath);
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var folderName = width > 16 ? "Small32" : "Small32";

            var possiblePaths = new[]
            {
                imagePath,
                Path.Combine(assemblyLocation ?? "", "Icons", "Small32", fileName),
                Path.Combine(assemblyLocation ?? "", "Icons", "Large512", fileName),
                Path.Combine(assemblyLocation ?? "", "Icons", fileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "Rhino.Inside.AutoCAD.bundle", "Icons", "Small32", fileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "Rhino.Inside.AutoCAD.bundle", "Icons", "Large512", fileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "Rhino.Inside.AutoCAD.bundle", "Icons", fileName),
                Path.Combine(@"e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\Icons", "Small32", fileName),
                Path.Combine(@"e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\Icons", "Large512", fileName)
            };

            foreach (var path in possiblePaths)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.DecodePixelWidth = width;
                    bitmap.DecodePixelHeight = height;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Updates the button images.
    /// </summary>
    private void UpdateButton(string buttonFilePath, RibbonButton button)
    {
        try
        {
            button.ShowImage = true;

            var smallImg = this.ResizeImage(buttonFilePath, _smallIconSize, _smallIconSize);
            if (smallImg != null)
            {
                button.Image = smallImg;
            }

            var largeImg = this.ResizeImage(buttonFilePath, _largeIconSize, _largeIconSize);
            if (largeImg != null)
            {
                button.LargeImage = largeImg;
            }
        }
        catch { }
    }

    /// <summary>
    /// Finds the Rhino Inside ribbon tab in the Autocad UI Ribbon.
    /// </summary>
    private bool FindRhinoInsideTab(out RibbonTab? rhinoInsideTab)
    {
        rhinoInsideTab = null;
        try
        {
            var ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return false;

            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Title == _rhinoInsideTabName || tab.Id == "RHINOINSIDE_TAB")
                {
                    rhinoInsideTab = tab;
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Finds the button in the Rhino Inside ribbon tab.
    /// </summary>
    private bool FindButton(RibbonTab rhinoInsideTab, out RibbonButton? ribbonButton)
    {
        ribbonButton = null;
        try
        {
            foreach (var panel in rhinoInsideTab.Panels)
            {
                foreach (var item in panel.Source.Items)
                {
                    if (item is RibbonButton button &&
                        button.Id == this.ButtonId)
                    {
                        ribbonButton = button;
                        return true;
                    }

                    if (item is RibbonRowPanel subPanel)
                    {
                        foreach (var subPanelItem in subPanel.Items)
                        {
                            if (subPanelItem is RibbonButton subButton &&
                                subButton.Id == this.ButtonId)
                            {
                                ribbonButton = subButton;
                                return true;
                            }
                        }
                    }
                }
            }
        }
        catch { }
        return false;
    }

    /// <inheritdoc />
    public void Replace(string buttonFilePath)
    {
        try
        {
            if (this.FindRhinoInsideTab(out var rhinoInsideTab) && rhinoInsideTab != null)
            {
                if (this.FindButton(rhinoInsideTab, out var button) && button != null)
                {
                    this.UpdateButton(buttonFilePath, button);
                }
            }
        }
        catch { }
    }
}