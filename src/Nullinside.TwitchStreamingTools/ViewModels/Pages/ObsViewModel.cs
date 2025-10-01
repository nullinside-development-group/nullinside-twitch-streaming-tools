using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using log4net;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles publishing your obs information to the website.
/// </summary>
public partial class ObsViewModel : PageViewModelBase {
  /// <summary>
  ///   The configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(ObsViewModel));

  /// <summary>
  ///   The obs websocket server address.
  /// </summary>
  [ObservableProperty] private string _obsServerAddress = "localhost:4455";

  /// <summary>
  ///   The obs websocket server password.
  /// </summary>
  [ObservableProperty] private string? _obsServerPassword;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ObsViewModel" /> class.
  /// </summary>
  /// <param name="configuration">The configuration.</param>
  public ObsViewModel(IConfiguration configuration) {
    _configuration = configuration;
    _obsServerAddress = configuration.ObsServerAddress ?? _obsServerAddress;
    _obsServerPassword = configuration.ObsServerPassword;
    this.PropertyChanged += OnPropertyChanged;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "ShareAndroidRegular";
  
  /// <summary>
  /// Updates the configuration.
  /// </summary>
  /// <param name="_">Unused sender.</param>
  /// <param name="e">The details on the property that changed.</param>
  private void OnPropertyChanged(object? _, PropertyChangedEventArgs e) {
    if (nameof(ObsServerAddress).Equals(e.PropertyName)) {
      _configuration.ObsServerAddress = ObsServerAddress;
    } else if (nameof(ObsServerPassword).Equals(e.PropertyName)) {
      _configuration.ObsServerPassword = ObsServerPassword;
    }

    _configuration.WriteConfiguration();
  }
}