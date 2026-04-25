using System;
using System.Threading.Tasks;
using log4net;

using Avalonia.Threading;

using Nullinside.Api.Common.Auth;
using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Utilities;

using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Nullinside.TwitchStreamingTools.Services;

/// <summary>
///   Manages the credentials in the application, ensuring credentials are kept up to date.
/// </summary>
public class TwitchAccountService : ITwitchAccountService {
  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOG = LogManager.GetLogger(typeof(TwitchAccountService));

  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The timer used to check the twitch OAuth token against the API.
  /// </summary>
  private readonly DispatcherTimer _timer;

  /// <summary>
  ///   The twitch chat client.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchAccountService" /> class.
  /// </summary>
  /// <param name="twitchClient">The twitch chat client.</param>
  /// <param name="configuration">The application configuration.</param>
  public TwitchAccountService(ITwitchClientProxy twitchClient, IConfiguration configuration) {
    _configuration = configuration;
    _twitchClient = twitchClient;
    _timer = new DispatcherTimer {
      Interval = TimeSpan.FromSeconds(5)
    };

    _timer.Tick += async (_, _) => await OnCheckCredentials().ConfigureAwait(false);
    _ = OnCheckCredentials();
  }

  /// <inheritdoc />
  public string? TwitchUsername { get; set; }

  /// <inheritdoc />
  public bool CredentialsAreValid { get; set; }

  /// <inheritdoc />
  public Action<bool>? OnCredentialsStatusChanged { get; set; }

  /// <inheritdoc />
  public Action<OAuthToken?>? OnCredentialsChanged { get; set; }

  /// <inheritdoc />
  public async Task UpdateCredentials(string bearer, string refresh, DateTime expires) {
    var oauth = new OAuthToken {
      AccessToken = bearer,
      RefreshToken = refresh,
      ExpiresUtc = expires
    };

    var twitchApi = new TwitchApiWrapper {
      OAuth = oauth
    };

    User? user = null;
    try {
      user = await twitchApi.GetUser().ConfigureAwait(false);
    }
    catch {
      // Do nothing
    }

    _configuration.OAuth = oauth;

    _configuration.TwitchUsername = user?.Login;
    _configuration.WriteConfiguration();
    _twitchClient.TwitchUsername = user?.Login;
    _twitchClient.TwitchOAuthToken = bearer;

    OnCredentialsChanged?.Invoke(oauth);
    await OnCheckCredentials().ConfigureAwait(false);
  }

  /// <inheritdoc />
  public void DeleteCredentials() {
    _configuration.OAuth = null;
    _configuration.TwitchUsername = null;
    _twitchClient.TwitchOAuthToken = null;
    _twitchClient.TwitchUsername = null;
    CredentialsAreValid = false;
    TwitchUsername = null;
    _configuration.WriteConfiguration();

    OnCredentialsChanged?.Invoke(null);
    OnCredentialsStatusChanged?.Invoke(false);
  }

  /// <summary>
  ///   Checks the OAuth token against the API to verify its validity.
  /// </summary>
  private async Task OnCheckCredentials() {
    _timer.Stop();
    // Grab the value so we can check if the value changed
    bool credsWereValid = CredentialsAreValid;

    try {
      // Refresh the token
      await DoTokenRefreshIfNearExpiration().ConfigureAwait(false);

      // Make sure the token works and get the user's login
      var twitchApi = new TwitchApiWrapper();
      User? user = await twitchApi.GetUser().ConfigureAwait(false);
      string? username = user?.Login;

      // Update the credentials
      CredentialsAreValid = !string.IsNullOrWhiteSpace(username);
      TwitchUsername = username;

      if (CredentialsAreValid && null != twitchApi.OAuth) {
        // Ensure the twitch client is using the latest credentials
        _twitchClient.TwitchUsername = username;
        _twitchClient.TwitchOAuthToken = twitchApi.OAuth.AccessToken;
      }
    }
    catch (Exception ex) {
      LOG.Error("Error checking credentials", ex);
      // Only delete credentials if they WERE valid and now we are sure they are definitely invalid.
      // A simple exception might be a network error.
      // If we want to be safe, we could check for specific exception types or just not delete here.
      // Given the logs, it seems this might be too aggressive.
    }
    finally {
      // Fire off the event if something changed
      if (credsWereValid != CredentialsAreValid) {
        OnCredentialsStatusChanged?.Invoke(CredentialsAreValid);
      }

      _timer.Start();
    }
  }

  /// <summary>
  ///   Checks the expiration of the OAuth token and refreshes if it's within 1 hour of the time.
  /// </summary>
  private async Task DoTokenRefreshIfNearExpiration() {
    var twitchApi = new TwitchApiWrapper();

    // Don't wait until the token is expired, refresh it ~1 hour before it expires 
    DateTime expiration = twitchApi.OAuth?.ExpiresUtc ?? DateTime.MaxValue;
    TimeSpan timeUntil = expiration - (DateTime.UtcNow + TimeSpan.FromHours(1));
    if (timeUntil.Ticks >= 0) {
      return;
    }

    if (null == twitchApi.OAuth || string.IsNullOrWhiteSpace(twitchApi.OAuth.AccessToken) ||
        string.IsNullOrWhiteSpace(twitchApi.OAuth.RefreshToken)) {
      return;
    }

    // Refresh the token
    await twitchApi.RefreshAccessToken().ConfigureAwait(false);

    // Update the configuration
    _configuration.OAuth = new OAuthToken {
      AccessToken = twitchApi.OAuth.AccessToken,
      RefreshToken = twitchApi.OAuth.RefreshToken,
      ExpiresUtc = twitchApi.OAuth.ExpiresUtc ?? DateTime.MinValue
    };
    _configuration.WriteConfiguration();
  }
}