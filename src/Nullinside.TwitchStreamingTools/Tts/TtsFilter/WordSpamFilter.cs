using System;
using System.Collections.Generic;
using System.Linq;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Filters out people that spam words and letters in the chat.
/// </summary>
public class WordSpamFilter : ITtsFilter {
  /// <summary>
  ///   The maximum number of duplicate letters to allow.
  /// </summary>
  /// <remarks>Very few words have a single letter repeated 3+ times in a row.</remarks>
  private const int MAXIMUM_LETTER_OCCURANCES = 2;

  /// <summary>
  ///   The maximum number of consecutive words that are the same.
  /// </summary>
  /// <remarks>Very few sentences require the use of the same word more than twice in a row.</remarks>
  private const int MAXIMUM_CONSECUTIVE_SAME_WORDS = 2;

  /// <summary>
  ///   Removes duplicate emotes from a message.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchInfo">The information on the original chat message.</param>
  /// <param name="username">The username of the twitch chatter for TTS to say.</param>
  /// <param name="currentMessage">The message from twitch chat.</param>
  /// <returns>The new TTS message and username.</returns>
  public Tuple<string, string> Filter(IConfiguration configuration, OnMessageReceivedArgs twitchInfo, string username, string currentMessage) {
    // See if a letter is being spammed over and over again in a single word
    List<string> parts = currentMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    for (int i = 0; i < parts.Count; i++) {
      List<char> part = parts[i].ToList();
      char previousLetter = ' ';
      uint consecutiveLetterCount = 1;
      for (int x = part.Count - 1; x >= 0; x--) {
        char letter = part[x];

        if (previousLetter.Equals(letter)) {
          ++consecutiveLetterCount;
        }
        else {
          consecutiveLetterCount = 1;
        }

        previousLetter = letter;
        if (consecutiveLetterCount > MAXIMUM_LETTER_OCCURANCES) {
          part.RemoveAt(x);
        }
      }

      parts[i] = string.Join("", part);
    }

    // Check the words in order and see if they spammed the same word more than once.
    uint wordRun = 0;
    string previous = string.Empty;
    for (int i = parts.Count - 1; i >= 0; i--) {
      string part = parts[i];
      if (part.Equals(previous)) {
        ++wordRun;

        if (wordRun >= MAXIMUM_CONSECUTIVE_SAME_WORDS) {
          parts.RemoveAt(i);
        }
      }
      else {
        wordRun = 0;
      }

      previous = part;
    }

    return new Tuple<string, string>(username, string.Join(" ", parts));
  }
}