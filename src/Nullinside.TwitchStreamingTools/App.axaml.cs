using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.ViewModels;
using Nullinside.TwitchStreamingTools.Views;

namespace Nullinside.TwitchStreamingTools;

/// <summary>
///   Main entry point of the application.
/// </summary>
public class App : Application {
  /// <summary>
  /// The debugging logger for the twitch client.
  /// </summary>
  private ILoggerFactory _loggerFactory;

  /// <summary>
  ///   Initializes the GUI.
  /// </summary>
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  /// <summary>
  ///   Launches the main application window.
  /// </summary>
  public override void OnFrameworkInitializationCompleted() {
    _loggerFactory = LoggerFactory.Create(c => c
        .AddConsole()
      //    .SetMinimumLevel(LogLevel.Trace) // uncomment to view raw messages received from twitch
    );

    TwitchClientProxy.Instance.TwitchOAuthToken = Configuration.Instance.OAuth?.AccessToken;
    TwitchClientProxy.Instance.TwitchUsername = Configuration.Instance.TwitchUsername;
    TwitchClientProxy.Instance.TwitchUsername = Configuration.Instance.TwitchUsername;

    // Register all the services needed for the application to run
    var collection = new ServiceCollection();
    collection.AddCommonServices();

    // Creates a ServiceProvider containing services from the provided IServiceCollection
    ServiceProvider services = collection.BuildServiceProvider();
    services.StartupServices();
    var vm = services.GetRequiredService<MainWindowViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      desktop.MainWindow = new MainWindow {
        DataContext = vm,
        ServiceProvider = services
      };
    }

    base.OnFrameworkInitializationCompleted();
  }

  /// <summary>
  ///   The handler for the "show" button on the taskbar context menu.
  /// </summary>
  /// <param name="sender">The invoker.</param>
  /// <param name="e">The event arguments.</param>
  private void ShowSettings_OnClick(object? sender, EventArgs e) {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      if (null == desktop.MainWindow) {
        return;
      }

      desktop.MainWindow.WindowState = WindowState.Normal;
    }
  }

  /// <summary>
  ///   The handler for the "close" button on the taskbar context menu.
  /// </summary>
  /// <param name="sender">The invoker.</param>
  /// <param name="e">The event arguments.</param>
  private void Close_OnClick(object? sender, EventArgs e) {
    Environment.Exit(0);
  }
}