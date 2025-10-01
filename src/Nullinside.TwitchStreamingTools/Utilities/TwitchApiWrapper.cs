using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using Newtonsoft.Json;

using Nullinside.Api.Common.Auth;
using Nullinside.Api.Common.Twitch;

namespace Nullinside.TwitchStreamingTools.Utilities;

/// <summary>
///   A wrapper for interacting with the Twitch Api.
/// </summary>
public class TwitchApiWrapper : TwitchApiProxy {
  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOG = LogManager.GetLogger(typeof(TwitchApiWrapper));

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchApiWrapper" /> class.
  /// </summary>
  public TwitchApiWrapper() : base(
    Configuration.Instance.OAuth?.AccessToken ?? "",
    Configuration.Instance.OAuth?.RefreshToken ?? "",
    Configuration.Instance.OAuth?.ExpiresUtc ?? DateTime.MinValue,
    Configuration.Instance.TwitchAppConfig?.ClientId ?? Constants.TWITCH_CLIENT_ID,
    Configuration.Instance.TwitchAppConfig?.ClientSecret,
    Configuration.Instance.TwitchAppConfig?.ClientRedirect ?? Constants.TWITCH_CLIENT_REDIRECT) {
  }

  /// <summary>
  ///   Handles refreshing the twitch oauth token either through a local application or through the website.
  /// </summary>
  /// <param name="token">The refresh token.</param>
  /// <returns>The new OAuth token information if successful, null otherwise.</returns>
  public override async Task<OAuthToken?> RefreshAccessToken(CancellationToken token = new()) {
    try {
      // If the secret is specified, then this isn't using our API to authenticate, it's using the twitch api directly.
      if (!string.IsNullOrWhiteSpace(TwitchAppConfig?.ClientSecret)) {
        await base.RefreshAccessToken(token).ConfigureAwait(false);
      }

      using var client = new HttpClient();
      string url = $"{Constants.API_SITE_DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools";
      var request = new HttpRequestMessage(HttpMethod.Post, url);
      var values = new Dictionary<string, string> { { "refreshToken", OAuth?.RefreshToken ?? string.Empty } };
      var content = new FormUrlEncodedContent(values);
      using HttpResponseMessage response = await client.PostAsync(request.RequestUri, content, token).ConfigureAwait(false);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
      var oauthResp = JsonConvert.DeserializeObject<OAuthToken>(responseBody);
      if (null == oauthResp) {
        return null;
      }

      return new OAuthToken {
        AccessToken = oauthResp.AccessToken,
        ExpiresUtc = oauthResp.ExpiresUtc,
        RefreshToken = oauthResp.RefreshToken
      };
    }
    catch (Exception e) {
      LOG.Error("Failed to refresh access token", e);
    }

    return null;
  }
}