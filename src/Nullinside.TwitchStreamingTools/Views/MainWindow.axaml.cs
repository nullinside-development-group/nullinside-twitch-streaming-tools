#if !DEBUG
using Microsoft.Extensions.DependencyInjection;

using Avalonia.Threading;

using Nullinside.TwitchStreamingTools.ViewModels;
#else
using Avalonia;
#endif
using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Nullinside.Api.Common.Desktop;

namespace Nullinside.TwitchStreamingTools.Views;

/// <summary>
///   The main application window.
/// </summary>
public partial class MainWindow : Window {
  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindow" /> class.
  /// </summary>
  public MainWindow() {
    InitializeComponent();
#if DEBUG
    this.AttachDevTools();
#endif
  }

  /// <summary>
  ///   The service provider for DI.
  /// </summary>
  public IServiceProvider? ServiceProvider { get; set; }

  /// <summary>
  ///   Checks for a new version number of the application.
  /// </summary>
  protected override void OnInitialized() {
    base.OnInitialized();

    // handle the command line arguments for updating the application if applicable.
    string[] args = Environment.GetCommandLineArgs();
    if (args.Contains("--update")) {
      _ = GitHubUpdateManager.PerformUpdateAndRestart("nullinside-development-group", "nullinside-twitch-streaming-tools", args[2].Trim('"').Trim(), "twitch-streaming-tools.zip");
      return;
    }

    if (args.Contains("--justUpdated")) {
      _ = GitHubUpdateManager.CleanupUpdate();
    }

    // check for a new version of the application.
    Task.Factory.StartNew(async () => {
      GithubLatestReleaseJson? serverVersion =
        await GitHubUpdateManager.GetLatestVersion("nullinside-development-group", "nullinside-twitch-streaming-tools").ConfigureAwait(false);
      if (null == serverVersion || string.IsNullOrWhiteSpace(serverVersion.name)) {
        return;
      }

      if (serverVersion.name?.Equals(Constants.APP_VERSION, StringComparison.InvariantCultureIgnoreCase) ?? true) {
// Had to add this because code clean up tools were removing the "redundant" return statement.
// which was causing the check to always be ignored.
#if !DEBUG
        return;
#endif
      }

#if !DEBUG
      var vm = ServiceProvider?.GetRequiredService<NewVersionWindowViewModel>();
      if (null == vm) {
        return;
      }

      vm.LocalVersion = Constants.APP_VERSION;
      Dispatcher.UIThread.Post(async void () => {
        try {
          var versionWindow = new NewVersionWindow {
            DataContext = vm
          };

          await versionWindow.ShowDialog(this).ConfigureAwait(false);
        }
        catch {
          // do nothing, don't crash
        }
      });
#endif
    });
  }
}