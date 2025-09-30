using System;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Filters out commands from being spoken in chat.
/// </summary>
public class CommandFilter : ITtsFilter {
  /// <summary>
  ///   Handles filtering commands from being spoken in chat.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchInfo">The information on the original chat message.</param>
  /// <param name="username">The username of the twitch chatter for TTS to say.</param>
  /// <param name="currentMessage">The message from twitch chat.</param>
  /// <returns>The new TTS message and username.</returns>
  public Tuple<string, string> Filter(IConfiguration configuration, OnMessageReceivedArgs twitchInfo, string username, string currentMessage) {
    if (currentMessage.StartsWith("!")) {
      currentMessage = "";
    }

    return new Tuple<string, string>(username, currentMessage);
  }
}