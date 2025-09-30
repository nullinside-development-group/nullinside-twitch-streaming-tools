using System;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Filters chat messages based on the users that sent them.
/// </summary>
internal class UsernameSkipFilter : ITtsFilter {
  /// <summary>
  ///   Filters out chat messages for bot users.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchInfo">The information on the original chat message.</param>
  /// <param name="username">The username of the twitch chatter for TTS to say.</param>
  /// <param name="currentMessage">The message from twitch chat.</param>
  /// <returns>The new TTS message and username.</returns>
  public Tuple<string, string> Filter(IConfiguration configuration, OnMessageReceivedArgs twitchInfo, string username, string currentMessage) {
    if (null == configuration.TtsUsernamesToSkip) {
      return new Tuple<string, string>(username, currentMessage);
    }

    foreach (string ignoredUser in configuration.TtsUsernamesToSkip) {
      if (ignoredUser.Equals(twitchInfo.ChatMessage.DisplayName, StringComparison.InvariantCultureIgnoreCase)) {
        return new Tuple<string, string>("", "");
      }
    }

    return new Tuple<string, string>(username, currentMessage);
  }
}