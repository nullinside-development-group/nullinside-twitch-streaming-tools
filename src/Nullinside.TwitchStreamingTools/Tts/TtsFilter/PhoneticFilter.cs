using System;
using System.Collections.Generic;
using System.Linq;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Converts things to their phonetic spellings.
/// </summary>
internal class PhoneticFilter : ITtsFilter {
  /// <summary>
  ///   Converts things to their phonetic spelling for TTS.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchInfo">The information on the original chat message.</param>
  /// <param name="username">The username of the twitch chatter for TTS to say.</param>
  /// <param name="currentMessage">The message from twitch chat.</param>
  /// <returns>The new TTS message and username.</returns>
  public Tuple<string, string> Filter(IConfiguration configuration, OnMessageReceivedArgs twitchInfo, string username, string currentMessage) {
    string message = currentMessage;

    if (null != configuration.TtsPhonetics) {
      foreach (KeyValuePair<string, string> usernameToPhonetic in configuration.TtsPhonetics) {
        message = message.Replace(usernameToPhonetic.Key, usernameToPhonetic.Value, StringComparison.InvariantCultureIgnoreCase);
      }

      KeyValuePair<string, string> foundUsername = configuration.TtsPhonetics.FirstOrDefault(k => twitchInfo.ChatMessage.DisplayName.Equals(k.Key, StringComparison.InvariantCultureIgnoreCase));
      if (!default(KeyValuePair<string, string>).Equals(foundUsername)) {
        username = foundUsername.Value;
      }
    }

    return new Tuple<string, string>(username, message);
  }
}