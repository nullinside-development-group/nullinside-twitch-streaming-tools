namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   The arguments to pass to sound stretcher when manipulating TTS audio.
/// </summary>
public class SoundStretchArgs {
  /// <summary>
  ///   Change sound tempo by n percents (-95 to +5000 %)
  /// </summary>
  public int? Tempo { get; set; }

  /// <summary>
  ///   Change sound pitch by n semitones (-60 to +60 semitones)
  /// </summary>
  public int? Pitch { get; set; }

  /// <summary>
  ///   Change sound rate by n percents (-95 to +5000 %)
  /// </summary>
  public int? Rate { get; set; }

  /// <summary>
  ///   Detect the BPM rate of sound and adjust tempo to meet 'n' BPMs. If value not specified, just detects the BPM rate.
  /// </summary>
  public int? Bpm { get; set; }

  /// <summary>
  ///   Use quicker tempo change algorithm (gain speed, lose quality)
  /// </summary>
  public bool Quick { get; set; }

  /// <summary>
  ///   Don't use anti-alias filtering (gain speed, lose quality)
  /// </summary>
  public bool AntiAliasingOff { get; set; }

  /// <summary>
  ///   Tune algorithm for speech processing (default is for music)
  /// </summary>
  public bool TurnOnSpeech { get; set; }
}