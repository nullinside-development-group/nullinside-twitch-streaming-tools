using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;

using Avalonia.Controls;

using Newtonsoft.Json;

using Nullinside.Api.Common.Auth;
using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;
using Nullinside.TwitchStreamingTools.Utilities;

namespace Nullinside.TwitchStreamingTools;

/// <summary>
///   The configuration of the application.
/// </summary>
public class Configuration : IConfiguration {
  /// <summary>
  ///   The location of the configuration file.
  /// </summary>
  private static readonly string CONFIG_LOCATION = Path.Combine(Constants.SAVE_FOLDER, "config.json");

  /// <summary>
  ///   The singleton instance.
  /// </summary>
  private static Configuration? s_instance;

  /// <summary>
  ///   Do not allow other people to create instances of our class.
  /// </summary>
  protected Configuration() {
  }

  /// <summary>
  ///   The singleton instance of the class.
  /// </summary>
  public static Configuration Instance {
    get {
      if (null == s_instance) {
        s_instance = ReadConfiguration() ?? new Configuration();
      }

      return s_instance;
    }
  }

  /// <inheritdoc />
  public string? ObsServerAddress { get; set; }
  
  /// <inheritdoc />
  public string? ObsServerPassword { get; set; }

  /// <summary>
  ///   The username of the user logged in through the <see cref="OAuth" /> token.
  /// </summary>
  public string? TwitchUsername { get; set; }

  /// <summary>
  ///   The twitch OAuth token.
  /// </summary>
  public OAuthToken? OAuth { get; set; }

  /// <summary>
  ///   The twitch application configuration for getting OAuth tokens.
  /// </summary>
  public TwitchAppConfig? TwitchAppConfig { get; set; }

  /// <summary>
  ///   The collection of twitch chats we should read from.
  /// </summary>
  public IEnumerable<TwitchChatConfiguration>? TwitchChats { get; set; }

  /// <summary>
  ///   The collection of usernames to skip reading the messages of.
  /// </summary>
  public IEnumerable<string>? TtsUsernamesToSkip { get; set; }

  /// <summary>
  ///   The collection of phonetic pronunciations of words.
  /// </summary>
  public IDictionary<string, string>? TtsPhonetics { get; set; }

  /// <summary>
  ///   The arguments to pass to Sound Stretcher to manipulate the TTS audio.
  /// </summary>
  public SoundStretchArgs? SoundStretchArgs { get; set; }

  /// <inheritdoc />
  public bool SayUsernameWithMessage { get; set; }

  /// <inheritdoc />
  public Keybind? SkipTtsKey { get; set; } = new() { Key = Keys.Pause };

  /// <inheritdoc />
  public Keybind? SkipAllTtsKey { get; set; }

  /// <summary>
  ///   Writes the configuration file to disk.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  public bool WriteConfiguration() {
    if (Design.IsDesignMode) {
      return false;
    }

    try {
      Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_LOCATION)!);

      string json = JsonConvert.SerializeObject(this, Formatting.Indented);
      File.WriteAllText(CONFIG_LOCATION, json);
      return true;
    }
    catch {
      return false;
    }
  }

  /// <summary>
  ///   Retrieves the default audio device configured in the application.
  /// </summary>
  /// <returns>The default audio device, if any audio device exists.</returns>
  public static string? GetDefaultAudioDevice() {
    return Instance.TwitchChats?.FirstOrDefault()?.OutputDevice ?? NAudioUtilities.GetDefaultOutputDevice();
  }

  /// <summary>
  ///   Retrieves the default TTS voice configured in the application.
  /// </summary>
  /// <returns>The default tts voice, if any tts voice exists.</returns>
  public static string? GetDefaultTtsVoice() {
    using var speech = new SpeechSynthesizer();
    return Instance.TwitchChats?.FirstOrDefault()?.TtsVoice ?? speech.GetInstalledVoices().FirstOrDefault()?.VoiceInfo.Name;
  }

  /// <summary>
  ///   Retrieves the default TTS volume configured in the application.
  /// </summary>
  /// <returns>The default tts voice, if any tts voice exists.</returns>
  public static uint? GetDefaultTtsVolume() {
    return Instance.TwitchChats?.FirstOrDefault()?.TtsVolume;
  }

  /// <summary>
  ///   Reads the configuration from disk.
  /// </summary>
  /// <returns>The configuration if successful, null otherwise.</returns>
  private static Configuration? ReadConfiguration() {
    try {
      string json = File.ReadAllText(CONFIG_LOCATION);
      return JsonConvert.DeserializeObject<Configuration>(json);
    }
    catch { return null; }
  }
}