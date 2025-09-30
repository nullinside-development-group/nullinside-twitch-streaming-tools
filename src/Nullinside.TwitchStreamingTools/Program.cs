using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;

using Avalonia;
using Avalonia.ReactiveUI;

using log4net;
using log4net.Config;

[assembly: SupportedOSPlatform("windows")]

namespace Nullinside.TwitchStreamingTools;

internal sealed class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.

  /// <summary>
  ///   The name used to ensure only a single instance of the application is running.
  /// </summary>
  private const string MutexName = "TwitchStreamingTools_SingleInstance";

  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

  /// <summary>
  ///   Used to prevent multiple instances of the application from running.
  /// </summary>
  private static Mutex? _mutex;

  /// <summary>
  ///   Main entrypoint of the application.
  /// </summary>
  /// <param name="args">The arguments passed to the application.</param>
  [STAThread]
  public static void Main(string[] args) {
#if DEBUG
    XmlConfigurator.Configure(new FileInfo("log4net.debug.config"));
#else
      XmlConfigurator.Configure(new FileInfo("log4net.config"));
#endif

    LOG.Info("Started application");

    AppDomain.CurrentDomain.UnhandledException += (_, exceptArgs) => {
      LOG.Fatal("Unhandled exception", exceptArgs.ExceptionObject as Exception);
    };

    // We only allow a single instance of the application to be launched at once (due to having only a single config
    // file)
    _mutex = new Mutex(true, MutexName, out bool onlyAppInstance);
    if (!onlyAppInstance) {
      LOG.Info("Application instance already running. Exiting.");
      return;
    }

    try {
      BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }
    finally {
      // Release the mutex when the application exits
      _mutex.ReleaseMutex();
      _mutex.Dispose();
    }
  }

  /// <summary>
  ///   Builds the avalonia application.
  /// </summary>
  /// <returns>The application builder.</returns>
  public static AppBuilder BuildAvaloniaApp() {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace()
      .UseReactiveUI();
  }
}