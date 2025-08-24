namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles setting up automation to be executed when certain events occur.
/// </summary>
public class AutomationViewModel : PageViewModelBase {
  /// <summary>
  ///   The application configuration;
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AutomationViewModel" /> class.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  public AutomationViewModel(IConfiguration configuration) {
    _configuration = configuration;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "RocketRegular";
}