using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nullinside.TwitchStreamingTools;

/// <summary>
///   Constants used throughout the application.
/// </summary>
public static class Constants {
#if DEBUG
  /// <summary>
  ///   The twitch app client id.
  /// </summary>
  public const string TWITCH_CLIENT_ID = "cvipqhi9y6ri8yhv0w8ryxokxh0ebd";

  /// <summary>
  ///   The domain that the api service is hosted at.
  /// </summary>
  public const string API_SITE_DOMAIN = $"http://{DOMAIN}";

  /// <summary>
  ///   The domain that the api service is hosted at.
  /// </summary>
  public const string DOMAIN = "localhost:5036";
#else
  /// <summary>
  ///   The twitch app client id.
  /// </summary>
  public const string TWITCH_CLIENT_ID = "gi1eu8xu9tl6vkjqz4tjqkdzfmcq5h";

  /// <summary>
  ///   The domain that the api service is hosted at.
  /// </summary>
  public const string API_SITE_DOMAIN = $"https://{DOMAIN}";
  
  /// <summary>
  ///   The domain that the api service is hosted at.
  /// </summary>
  public const string DOMAIN = "nullinside.com";
#endif

  /// <summary>
  ///   The location of saved files the application generates.
  /// </summary>
  /// <remarks>Not the install folder.</remarks>
  public static readonly string SAVE_FOLDER =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nullinside",
      "twitch-streaming-tools");

  /// <summary>
  ///   A regular expression for identifying a link.
  /// </summary>
  public const string REGEX_URL = @"(https?:\/\/(www\.)?)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=,]*)";

  /// <summary>
  ///   The twitch app redirect link.
  /// </summary>
  public const string TWITCH_CLIENT_REDIRECT = $"{API_SITE_DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools";

  /// <summary>
  ///   The version of the application being run right now.
  /// </summary>
  public static readonly string? APP_VERSION = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()[..^2];

  /// <summary>
  ///   The default bot list to populate when a user doesn't any have bots configured.
  /// </summary>
  public static readonly IEnumerable<string> TWITCH_DEFAULT_BOT_LIST = new[] {
    "streamelements",
    "nightbot",
    "sery_bot",
    "wizebot",
    "kofistreambot",
    "tangiabot",
    "botrixoficial",
    "moobot",
    "own3d",
    "creatisbot",
    "frostytoolsdotcom",
    "streamlabs",
    "pokemoncommunitygame",
    "fossabot"
  };

  /// <summary>
  ///   The twitch permissions to request.
  /// </summary>
  public static readonly IEnumerable<string> TWITCH_SCOPES = new[] {
    "analytics:read:extensions",
    "analytics:read:games",
    "bits:read",
    "channel:edit:commercial",
    "channel:manage:broadcast",
    "channel:manage:extensions",
    "channel:manage:polls",
    "channel:manage:predictions",
    "channel:manage:redemptions",
    "channel:manage:schedule",
    "channel:manage:videos",
    "channel:read:editors",
    "channel:read:goals",
    "channel:read:hype_train",
    "channel:read:polls",
    "channel:read:predictions",
    "channel:read:redemptions",
    "channel:read:stream_key",
    "channel:read:subscriptions",
    "clips:edit",
    "moderation:read",
    "moderator:manage:automod",
    "moderator:read:chatters",
    "user:edit",
    "user:edit:follows",
    "user:manage:blocked_users",
    "user:read:blocked_users",
    "user:read:broadcast",
    "user:read:email",
    "user:read:follows",
    "user:read:subscriptions",
    "channel_subscriptions",
    "channel_commercial",
    "channel_editor",
    "user_follows_edit",
    "channel_read",
    "user_read",
    "user_blocks_read",
    "user_blocks_edit",
    "channel:moderate",
    "chat:edit",
    "chat:read",
    "whispers:read",
    "whispers:edit"
  };
}