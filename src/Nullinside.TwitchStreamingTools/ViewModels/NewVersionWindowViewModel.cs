using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Nullinside.Api.Common.Desktop;
using Nullinside.TwitchStreamingTools.Views;

namespace Nullinside.TwitchStreamingTools.ViewModels;

/// <summary>
///   The view model for the <seealso cref="NewVersionWindow" /> class.
/// </summary>
public partial class NewVersionWindowViewModel : ViewModelBase {
  /// <summary>
  ///   True if updating the application currently, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _isUpdating;

  /// <summary>
  ///   The local version of the software.
  /// </summary>
  private string? _localVersion;

  /// <summary>
  ///   The url for the new version's assets on GitHub.
  /// </summary>
  private string? _newVersionUrl;

  /// <summary>
  ///   The version of the application on the GitHub server.
  /// </summary>
  [ObservableProperty] private string? _serverVersion;

  /// <summary>
  ///   Initializes a new instance of the <see cref="NewVersionWindowViewModel" /> class.
  /// </summary>
  public NewVersionWindowViewModel() {
    // asynchronously determine the current version number.
    Task.Factory.StartNew(async () => {
      GithubLatestReleaseJson? version =
        await GitHubUpdateManager.GetLatestVersion("nullinside-development-group", "nullinside-twitch-streaming-tools").ConfigureAwait(false);

      if (null == version) {
        return;
      }

      _newVersionUrl = version.html_url;
      Dispatcher.UIThread.Post(() => ServerVersion = version.name ?? string.Empty);
    });
  }

  /// <summary>
  ///   The local version of the software.
  /// </summary>
  public string? LocalVersion {
    get => _localVersion;
    set => _localVersion = value;
  }

  /// <summary>
  ///   A command to close the current window.
  /// </summary>
  /// <param name="self">The reference to our own window.</param>
  [RelayCommand]
  private void CloseWindow(Window self) {
    self.Close();
  }

  /// <summary>
  ///   Launches the web browser at the new release page.
  /// </summary>
  [RelayCommand]
  private void StartUpdateSoftware() {
    IsUpdating = true;
    GitHubUpdateManager.PrepareUpdate()
      .ContinueWith(_ => {
        if (string.IsNullOrWhiteSpace(_newVersionUrl)) {
          return;
        }

        Process.Start("explorer", _newVersionUrl);
        GitHubUpdateManager.ExitApplicationToUpdate();
      }).ConfigureAwait(false);
  }
}