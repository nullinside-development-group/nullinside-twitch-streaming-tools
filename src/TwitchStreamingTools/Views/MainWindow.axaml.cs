using System;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

using Nullinside.Api.Common.Desktop;
#if !DEBUG
using TwitchStreamingTools.ViewModels;
#endif

namespace TwitchStreamingTools.Views;

/// <summary>
///   The main application window.
/// </summary>
public partial class MainWindow : Window {
  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindow" /> class.
  /// </summary>
  public MainWindow() {
    InitializeComponent();
    Constants.CLIPBOARD = Clipboard;
#if DEBUG
    this.AttachDevTools();
#endif
  }

  /// <summary>
  ///   Checks for a new version number of the application.
  /// </summary>
  protected override void OnInitialized() {
    base.OnInitialized();

    Task.Factory.StartNew(async () => {
      GithubLatestReleaseJson? serverVersion =
        await GitHubUpdateManager.GetLatestVersion("nullinside-development-group", "twitch-streaming-tools");
      string? localVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
      if (null == serverVersion || string.IsNullOrWhiteSpace(serverVersion.name) ||
          string.IsNullOrWhiteSpace(localVersion)) {
        return;
      }

      localVersion = localVersion.Substring(0, localVersion.LastIndexOf('.'));
      if (string.IsNullOrWhiteSpace(localVersion)) {
        return;
      }

      if (serverVersion.name?.Equals(localVersion, StringComparison.InvariantCultureIgnoreCase) ?? true) {
        return;
      }

      Dispatcher.UIThread.Post(async () => {
#if !DEBUG
        var versionWindow = new NewVersionWindow {
          DataContext = new NewVersionWindowViewModel {
            LocalVersion = localVersion
          }
        };

        await versionWindow.ShowDialog(this);
#endif
      });
    });
  }
}