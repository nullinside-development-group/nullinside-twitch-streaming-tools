using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using log4net;

using Newtonsoft.Json;

using Nullinside.Api.Common;
using Nullinside.Api.Common.Extensions;
using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Services;
using Nullinside.TwitchStreamingTools.Utilities;

using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your account to the application.
/// </summary>
public partial class AccountViewModel : PageViewModelBase {
  /// <summary>
  ///   The path to the folder containing cached profile images.
  /// </summary>
  private static readonly string PROFILE_IMAGE_FOLDER = Path.Combine(Constants.SAVE_FOLDER,
    "twitch-profile-image-cache");

  /// <summary>
  ///   The template for a profile image filename.
  /// </summary>
  private static readonly string PROFILE_IMAGE_FILENAME = "twitch_profile_{0}.png";

  /// <summary>
  ///   The configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(AccountViewModel));

  /// <summary>
  ///   Manages the account OAuth information.
  /// </summary>
  private readonly ITwitchAccountService _twitchAccountService;

  /// <summary>
  ///   True if currently downloading the logged in user's profile photo, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _downloadingProfileImage;

  /// <summary>
  ///   True if we have a valid OAuth token, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _hasValidOAuthToken;

  /// <summary>
  ///   True if currently in the logging in process, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _loggingIn;

  /// <summary>
  ///   The profile image of the logged in user.
  /// </summary>
  [ObservableProperty] private Bitmap? _profileImage;

  /// <summary>
  ///   The authenticated user's twitch username.
  /// </summary>
  [ObservableProperty] private string? _twitchUsername;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AccountViewModel" /> class.
  /// </summary>
  /// <param name="twitchAccountService">Manages the account OAuth information.</param>
  /// <param name="configuration">The configuration.</param>
  public AccountViewModel(ITwitchAccountService twitchAccountService, IConfiguration configuration) {
    _twitchAccountService = twitchAccountService;
    _twitchAccountService.OnCredentialsStatusChanged += OnCredentialsStatusChanged;
    _twitchAccountService.OnCredentialsChanged += OnCredentialsChanged;
    _configuration = configuration;

    // Set the initial state of the ui
    HasValidOAuthToken = _twitchAccountService.CredentialsAreValid;
    TwitchUsername = _twitchAccountService.TwitchUsername;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "InprivateAccountRegular";

  /// <summary>
  ///   The application version number.
  /// </summary>
  public string? Version => Constants.APP_VERSION;

  /// <summary>
  ///   Loads the profile image when the UI loads.
  /// </summary>
  public override async void OnLoaded() {
    base.OnLoaded();

    try {
      await LoadProfileImage().ConfigureAwait(false);
    }
    catch (Exception ex) {
      _logger.Error("Failed to load profile image", ex);
    }
  }

  /// <summary>
  ///   Finds the profile image locally or downloads it.
  /// </summary>
  private async Task LoadProfileImage() {
    // Try to get the file locally.
    string? profileImagePath = string.Format(PROFILE_IMAGE_FILENAME, _configuration.TwitchUsername);
    profileImagePath = Path.Combine(PROFILE_IMAGE_FOLDER, profileImagePath);
    if (File.Exists(profileImagePath)) {
      ProfileImage = new Bitmap(profileImagePath);
      return;
    }

    // If we couldn't find the file, download it.
    DownloadingProfileImage = true;
    profileImagePath = await DownloadUserImage().ConfigureAwait(false);
    if (null == profileImagePath) {
      return;
    }

    ProfileImage = new Bitmap(profileImagePath);
    DownloadingProfileImage = false;
  }

  /// <summary>
  ///   Called when the credentials are changed to load the new profile image.
  /// </summary>
  /// <param name="token"></param>
  private async void OnCredentialsChanged(TwitchAccessToken? token) {
    try {
      if (string.IsNullOrWhiteSpace(token?.AccessToken)) {
        ProfileImage = null;
        return;
      }

      await LoadProfileImage().ConfigureAwait(false);
    }
    catch (Exception ex) {
      _logger.Error("Failed to download user profile image", ex);
    }
  }

  /// <summary>
  ///   Invoked when the status of the credentials changes from the <seealso cref="ITwitchAccountService" />.
  /// </summary>
  /// <param name="valid">True if the credentials are valid, false otherwise.</param>
  private void OnCredentialsStatusChanged(bool valid) {
    try {
      if (!valid) {
        HasValidOAuthToken = false;
        TwitchUsername = null;
        return;
      }

      HasValidOAuthToken = true;
      TwitchUsername = _twitchAccountService.TwitchUsername;
    }
    catch (Exception ex) {
      _logger.Error("Failed to update credentials status", ex);
    }
  }

  /// <summary>
  ///   Launches the computer's default browser to generate an OAuth token.
  /// </summary>
  [RelayCommand]
  private async Task PerformLogin() {
    LoggingIn = true;
    try {
      CancellationToken token = CancellationToken.None;

      // Create an identifier for this credential request.
      var guid = Guid.NewGuid();

      // Create a web socket connection to the api which will provide us with the credentials from twitch.
      ClientWebSocket webSocket = new();
      await webSocket.ConnectAsync(new Uri($"ws://{Constants.DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools/ws"), token).ConfigureAwait(false);
      await webSocket.SendTextAsync(guid.ToString(), token).ConfigureAwait(false);

      // Launch the web browser to twitch to ask for account permissions. Twitch will be instructed to callback to our
      // api (redirect_uri) which will give us the credentials on the web socket above.
      string url = $"https://id.twitch.tv/oauth2/authorize?client_id={Constants.TWITCH_CLIENT_ID}&" +
                   $"redirect_uri={Constants.TWITCH_CLIENT_REDIRECT}&" +
                   "response_type=code&" +
                   $"scope={string.Join("%20", Constants.TWITCH_SCOPES)}&" +
                   $"state={guid}";
      Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });

      // Wait for the user to finish giving us permission on the website. Once they provide us access we will receive
      // a response on the web socket containing a JSON with our OAuth information.
      string json = await webSocket.ReceiveTextAsync(token).ConfigureAwait(false);

      // Close the connection, both sides will be waiting to do this so we do it immediately.
      await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed Successfully!", token).ConfigureAwait(false);

      // Update the oauth token in the twitch account service. 
      var oauthResp = JsonConvert.DeserializeObject<TwitchAccessToken>(json);
      if (null == oauthResp || null == oauthResp.AccessToken || null == oauthResp.RefreshToken || null == oauthResp.ExpiresUtc) {
        _logger.Error($"Failed to get a valid oauth token, got: {json}");
        return;
      }

      await _twitchAccountService.UpdateCredentials(oauthResp.AccessToken, oauthResp.RefreshToken, oauthResp.ExpiresUtc.Value).ConfigureAwait(false);
    }
    catch (Exception ex) {
      _logger.Error("Failed to launch browser to login", ex);
    }
    finally {
      LoggingIn = false;
    }
  }

  /// <summary>
  ///   Clears the credentials out (logging out).
  /// </summary>
  [RelayCommand]
  private void ClearCredentials() {
    _twitchAccountService.DeleteCredentials();
  }

  /// <summary>
  ///   Downloads the user's profile image and adds it to the cache.
  /// </summary>
  /// <returns>The path to the saved file.</returns>
  private async Task<string?> DownloadUserImage(CancellationToken token = new()) {
    return await Retry.Execute(async () => {
      // The user object from the API will tell us the download link on twitch for the image.
      var api = new TwitchApiWrapper();
      if (string.IsNullOrWhiteSpace(api.OAuth?.AccessToken)) {
        return null;
      }

      User? user = await api.GetUser(token).ConfigureAwait(false);
      if (string.IsNullOrWhiteSpace(user?.ProfileImageUrl)) {
        return null;
      }

      // Download the image via http.
      using var http = new HttpClient();
      byte[] imageBytes = await http.GetByteArrayAsync(user.ProfileImageUrl, token).ConfigureAwait(false);

      // If the directory doesn't exist, create it.
      if (!Directory.Exists(PROFILE_IMAGE_FOLDER)) {
        Directory.CreateDirectory(PROFILE_IMAGE_FOLDER);
      }

      // I don't think twitch usernames can have non-filepath friendly characters but might as well sanitize it anyway.
      string filename = SanitizeFilename(string.Format(PROFILE_IMAGE_FILENAME, user.Login));
      string imagePath = Path.Combine(PROFILE_IMAGE_FOLDER, filename);

      // Save to disk
      await File.WriteAllBytesAsync(imagePath, imageBytes, token).ConfigureAwait(false);

      // Return path to file, even though everyone already knows it.
      return imagePath;
    }, 10, token, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
  }

  /// <summary>
  ///   Removes invalid characters from the passed in string.
  /// </summary>
  /// <param name="input">The filename to sanitize.</param>
  /// <returns>The sanitized filename.</returns>
  private static string SanitizeFilename(string input) {
    foreach (char c in Path.GetInvalidFileNameChars()) {
      input = input.Replace(c, '_');
    }

    return input;
  }
}