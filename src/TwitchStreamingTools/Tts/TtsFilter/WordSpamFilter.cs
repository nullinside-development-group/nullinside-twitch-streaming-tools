﻿using System;
using System.Collections.Generic;
using System.Linq;

using TwitchLib.Client.Events;

namespace TwitchStreamingTools.Tts.TtsFilter;

/// <summary>
///   Filters out people that spam words and letters in the chat.
/// </summary>
public class WordSpamFilter : ITtsFilter {
  /// <summary>
  ///   The maximum number of emotes to allow.
  /// </summary>
  private const int MAXIMUM_LETTER_OCCURANCES = 2;

  /// <summary>
  ///   The maximum number of emotes to allow.
  /// </summary>
  private const int MAXIMUM_CONSECUTIVE_SAME_WORDS = 2;

  /// <summary>
  ///   Replacement text so I know something was said that Cathy didn't like.
  /// </summary>
  private const string SPAM_REPLACEMENT = "I am a fucking moron that spams the chat";

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
    string[] parts = currentMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    for (int i = 0; i < parts.Length; i++) {
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
    foreach (string part in parts) {
      if (part.Equals(previous)) {
        ++wordRun;

        if (wordRun > MAXIMUM_CONSECUTIVE_SAME_WORDS) {
          return new Tuple<string, string>(username, SPAM_REPLACEMENT);
        }
      }
      else {
        wordRun = 0;
      }

      previous = part;
    }

    //// Check for phrases that are repeated
    //var allEmotes = new List<string>();
    //allEmotes.AddRange(EmoteLookup.GetBetterTtvEmotes(twitchInfo.ChatMessage.Channel));
    //allEmotes.AddRange(EmoteLookup.GetFrankerzFaceEmotes(twitchInfo.ChatMessage.Channel));

    //foreach (var emote in allEmotes) {
    //    currentMessage = currentMessage.Replace(emote, "");
    //}

    //for (int i = 0; i < parts.Length; i++) {
    //    var startingWord = parts[i];
    //    var currentPhrase = parts[i];

    //    int x;
    //    for (x = i; x < parts.Length; x++) {
    //        var nextWord = parts[x];
    //        if (startingWord.Equals(nextWord)) {
    //            currentPhrase.Count()
    //        }

    //        currentPhrase += $" {nextWord}";
    //    }
    //}

    return new Tuple<string, string>(username, string.Join(" ", parts));
  }
}