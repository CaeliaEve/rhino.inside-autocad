using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.UI.Resources.Models;
using System.Collections.ObjectModel;

namespace Rhino.Inside.AutoCAD.UI.Resources.ViewModels;

/// <summary>
/// The view model for the Rhino version selection dialog.
/// </summary>
public partial class RhinoVersionDialogViewModel : ObservableObject
{
    private const string _useVersionButtonFormat = UIConstants.UseVersionButtonFormat;
    private const string _alwaysUseVersionButtonFormat = UIConstants.AlwaysUseVersionButtonFormat;
    private const string _rhinoFallbackName = UIConstants.RhinoFallbackName;

    /// <summary>
    /// The Rhino installations the user can choose between, ordered newest first.
    /// </summary>
    public ObservableCollection<IRhinoInstallation> Installations { get; }

    /// <summary>
    /// The installation the user currently has selected. Null only if the list is
    /// deselected, in which case neither of the two accepting commands can run.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UseButtonText))]
    [NotifyPropertyChangedFor(nameof(AlwaysUseButtonText))]
    [NotifyCanExecuteChangedFor(nameof(UseCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlwaysUseCommand))]
    private IRhinoInstallation? _selectedInstallation;

    /// <summary>
    /// The choice the user made. Defaults to <see cref="RhinoVersionChoice.Cancel"/> so
    /// closing the window by any other means is treated as a cancellation.
    /// </summary>
    public RhinoVersionChoice Choice { get; private set; } = RhinoVersionChoice.Cancel;

    /// <summary>
    /// Raised when the user has made a choice and the window should close.
    /// </summary>
    public event EventHandler? ChoiceMade;

    /// <summary>
    /// The label of the button which uses the selected version for this session only.
    /// </summary>
    public string UseButtonText => string.Format(_useVersionButtonFormat,
        this.SelectedInstallation?.DisplayName ?? _rhinoFallbackName);

    /// <summary>
    /// The label of the button which uses the selected version and stops asking.
    /// </summary>
    public string AlwaysUseButtonText => string.Format(_alwaysUseVersionButtonFormat,
        this.SelectedInstallation?.DisplayName ?? _rhinoFallbackName);

    /// <summary>
    /// Constructs a new <see cref="RhinoVersionDialogViewModel"/>.
    /// </summary>
    /// <param name="installations">The installations to choose between, newest first.</param>
    /// <param name="preselected">
    /// The installation to select initially, or null to select the first.
    /// </param>
    public RhinoVersionDialogViewModel(
        IReadOnlyList<IRhinoInstallation> installations,
        IRhinoInstallation? preselected = null)
    {
        if (installations.Count == 0)
            throw new ArgumentException(
                "At least one Rhino installation is required.", nameof(installations));

        this.Installations = new ObservableCollection<IRhinoInstallation>(installations);

        _selectedInstallation = preselected ?? installations[0];
    }

    /// <summary>
    /// True when a version is selected and so can be accepted.
    /// </summary>
    private bool CanAcceptSelection() => this.SelectedInstallation != null;

    /// <summary>
    /// Uses the selected version for this session only.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAcceptSelection))]
    private void Use() => this.Complete(RhinoVersionChoice.Use);

    /// <summary>
    /// Uses the selected version and stops asking on subsequent sessions.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAcceptSelection))]
    private void AlwaysUse() => this.Complete(RhinoVersionChoice.AlwaysUse);

    /// <summary>
    /// Cancels without choosing a version.
    /// </summary>
    [RelayCommand]
    private void Cancel() => this.Complete(RhinoVersionChoice.Cancel);

    /// <summary>
    /// Records the choice and signals the window to close.
    /// </summary>
    /// <param name="choice">The choice the user made.</param>
    private void Complete(RhinoVersionChoice choice)
    {
        this.Choice = choice;

        this.ChoiceMade?.Invoke(this, EventArgs.Empty);
    }
}
