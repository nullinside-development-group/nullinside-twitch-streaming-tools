using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools.Tts;

/// <summary>
///   The contract for a class that monitors twitch chat and converts the messages sent to TTS.
/// </summary>
public interface ITwitchChatTts {
  /// <summary>
  ///   The configuration for the twitch chat.
  /// </summary>
  TwitchChatConfiguration? ChatConfig { get; }

  /// <summary>
  ///   The username being used to connect to twitch chat.
  /// </summary>
  string? CurrentUsername { get; set; }

  /// <summary>
  ///   Releases unmanaged resources.
  /// </summary>
  void Dispose();

  /// <summary>
  ///   Connects to the chat to listen for messages to read in text to speech.
  /// </summary>
  void Connect();

  /// <summary>
  ///   Pauses the text to speech.
  /// </summary>
  void Pause();

  /// <summary>
  ///   Continues the text to speech.
  /// </summary>
  void Unpause();

  /// <summary>
  ///   Skips the current TTS.
  /// </summary>
  void SkipCurrentTts();

  /// <summary>
  ///   Skips all TTSes in the queue.
  /// </summary>
  void SkipAllTts();
}