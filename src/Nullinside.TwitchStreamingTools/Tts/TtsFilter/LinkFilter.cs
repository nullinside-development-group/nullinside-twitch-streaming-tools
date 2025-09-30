using System;
using System.Text.RegularExpressions;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Filters out links from being read.
/// </summary>
internal class LinkFilter : ITtsFilter {
  /// <summary>
  ///   Filters out links from text to speech.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchInfo">The information on the original chat message.</param>
  /// <param name="username">The username of the twitch chatter for TTS to say.</param>
  /// <param name="currentMessage">The message from twitch chat.</param>
  /// <returns>The new TTS message and username.</returns>
  public Tuple<string, string> Filter(IConfiguration configuration, OnMessageReceivedArgs twitchInfo, string username, string currentMessage) {
    // Protect against removing ellipsis
    MatchCollection matches = Regex.Matches(currentMessage, Constants.REGEX_URL, RegexOptions.CultureInvariant);
    foreach (Match match in matches) {
      if (!match.Value.Contains("..")) {
        currentMessage = currentMessage.Replace(match.Value, "");
      }
    }

    return new Tuple<string, string>(username, currentMessage);
  }
}