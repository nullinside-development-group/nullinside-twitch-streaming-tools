namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   The twitch chat TTS configuration
/// </summary>
public class TwitchChatConfiguration {
  /// <summary>
  ///   Gets or sets the output device to send audio to.
  /// </summary>
  public string? OutputDevice { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether text to speech is on.
  /// </summary>
  public bool TtsOn { get; set; }

  /// <summary>
  ///   Gets or sets the selected Microsoft Text to Speech voice.
  /// </summary>
  public string? TtsVoice { get; set; }

  /// <summary>
  ///   Gets or sets the volume of the text to speech voice.
  /// </summary>
  public uint TtsVolume { get; set; }

  /// <summary>
  ///   Gets or sets the twitch channel to read chat from.
  /// </summary>
  public string? TwitchChannel { get; set; }
}