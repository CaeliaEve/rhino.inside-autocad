using CommunityToolkit.Mvvm.ComponentModel;

namespace Rhino.Inside.AutoCAD.UI.Resources.ViewModels;

/// <summary>
/// The view model for the expiration dialog.
/// </summary>
public partial class ExpirationDialogViewModel : ObservableObject
{
    /// <summary>
    /// The expiration message to display to the user.
    /// </summary>
    [ObservableProperty]
    private string _expirationMessage;

    /// <summary>
    /// The version of the application.
    /// </summary>
    [ObservableProperty]
    private string _appVersion;

    /// <summary>
    /// The copyright notice which appears on the dialog.
    /// </summary>
    [ObservableProperty]
    private string _copyrightNotice;

    /// <summary>
    /// The URL to download the latest version.
    /// </summary>
    public string DownloadUrl { get; }

    /// <summary>
    /// Constructs a new <see cref="ExpirationDialogViewModel"/>.
    /// </summary>
    /// <param name="expirationMessage">The expiration message to display.</param>
    /// <param name="downloadUrl">The URL to download the latest version.</param>
    /// <param name="appVersion">The current application version.</param>
    public ExpirationDialogViewModel(string expirationMessage, string downloadUrl, string appVersion)
    {
        _expirationMessage = expirationMessage;
        _appVersion = $"Software version {appVersion}";
        _copyrightNotice = $"Copyright {DateTime.Now.Year} ©";
        DownloadUrl = downloadUrl;
    }
}
