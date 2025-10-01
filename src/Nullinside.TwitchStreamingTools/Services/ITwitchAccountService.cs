using System;
using System.Threading.Tasks;

using Nullinside.Api.Common.Auth;
using Nullinside.Api.Common.Twitch;

namespace Nullinside.TwitchStreamingTools.Services;

/// <summary>
///   The contract for credential management in the application.
/// </summary>
public interface ITwitchAccountService {
  /// <summary>
  ///   The current oauth token's twitch username.
  /// </summary>
  string? TwitchUsername { get; set; }

  /// <summary>
  ///   A flag indicating whether the credentials are currently valid.
  /// </summary>
  bool CredentialsAreValid { get; set; }

  /// <summary>
  ///   An event indicating that the current status of the credentials has changed.
  /// </summary>
  Action<bool>? OnCredentialsStatusChanged { get; set; }

  /// <summary>
  ///   An event indicating that the credentials have changed.
  /// </summary>
  Action<OAuthToken?>? OnCredentialsChanged { get; set; }

  /// <summary>
  ///   Updates the credentials.
  /// </summary>
  /// <param name="bearer">The bearer token.</param>
  /// <param name="refresh">The refresh token.</param>
  /// <param name="expires">The <seealso cref="DateTime" /> when the credentials expire.</param>
  Task UpdateCredentials(string bearer, string refresh, DateTime expires);

  /// <summary>
  ///   Clears out the credentials.
  /// </summary>
  void DeleteCredentials();
}