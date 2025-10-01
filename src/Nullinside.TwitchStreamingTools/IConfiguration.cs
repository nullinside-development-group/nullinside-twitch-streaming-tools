using System.Collections.Generic;

using Nullinside.Api.Common.Auth;
using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools;

/// <summary>
///   The contract for the configuration of the application.
/// </summary>
public interface IConfiguration {
  /// <summary>
  ///   The username of the user logged in through the <see cref="OAuth" /> token.
  /// </summary>
  string? TwitchUsername { get; set; }

  /// <summary>
  ///   The twitch OAuth token.
  /// </summary>
  OAuthToken? OAuth { get; set; }

  /// <summary>
  ///   The twitch application configuration for getting OAuth tokens.
  /// </summary>
  TwitchAppConfig? TwitchAppConfig { get; set; }

  /// <summary>
  ///   The collection of twitch chats we should read from.
  /// </summary>
  IEnumerable<TwitchChatConfiguration>? TwitchChats { get; set; }

  /// <summary>
  ///   The collection of usernames to skip reading the messages of.
  /// </summary>
  IEnumerable<string>? TtsUsernamesToSkip { get; set; }

  /// <summary>
  ///   The collection of phonetic pronunciations of words.
  /// </summary>
  IDictionary<string, string>? TtsPhonetics { get; set; }

  /// <summary>
  ///   The arguments to pass to sound stretch to manipulate TTS audio.
  /// </summary>
  SoundStretchArgs? SoundStretchArgs { get; set; }

  /// <summary>
  ///   True if "username says message" should be used as the template for TTS messages, false to read just the message.
  /// </summary>
  bool SayUsernameWithMessage { get; set; }

  /// <summary>
  ///   The key press to skip the TTS.
  /// </summary>
  Keybind? SkipTtsKey { get; set; }

  /// <summary>
  ///   The key press to skip all the TTS.
  /// </summary>
  Keybind? SkipAllTtsKey { get; set; }

  /// <summary>
  ///   Writes the configuration file to disk.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  bool WriteConfiguration();
}