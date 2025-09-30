using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Nullinside.TwitchStreamingTools.Utilities;

/// <summary>
///   Handles queuing sounds to play.
/// </summary>
public class GlobalSoundManager {
  /// <summary>
  ///   The singleton instance of the class.
  /// </summary>
  private static GlobalSoundManager? s_instance;

  /// <summary>
  ///   The sentinel that indicates it's time to exit the <see cref="_soundPlayThread" /> thread.
  /// </summary>
  private readonly SoundPlayingWrapper _exitSentinel;

  /// <summary>
  ///   The thread responsible for playing sounds that are queued.
  /// </summary>
  private readonly Thread _soundPlayThread;

  /// <summary>
  ///   The collection that contains the sounds to play.
  /// </summary>
  private readonly BlockingCollection<SoundPlayingWrapper> _soundsToPlay;

  /// <summary>
  ///   Initializes a new instance of the <see cref="GlobalSoundManager" /> class.
  /// </summary>
  protected GlobalSoundManager() {
    _exitSentinel = new SoundPlayingWrapper(string.Empty, string.Empty, -1, null);
    _soundsToPlay = new BlockingCollection<SoundPlayingWrapper>();
    _soundPlayThread = new Thread(SoundPlayThreadMain);
    _soundPlayThread.IsBackground = true;
    _soundPlayThread.Start();
  }

  /// <summary>
  ///   Gets the singleton instance of the class.
  /// </summary>
  public static GlobalSoundManager Instance {
    get {
      if (null == s_instance) {
        s_instance = new GlobalSoundManager();
      }

      return s_instance;
    }
  }

  /// <summary>
  ///   Gets or sets a value indicating whether we are currently playing a sound.
  /// </summary>
  public bool CurrentlyPlayingSound { get; set; }

  /// <summary>
  ///   Adds a sound to play.
  /// </summary>
  /// <param name="filename">The name of the file to play.</param>
  /// <param name="outputDevice">The output device to play the file on.</param>
  /// <param name="volume">The volume to play the file at.</param>
  /// <param name="callback">The callback in invoke after the sound is played.</param>
  public void QueueSound(string filename, string outputDevice, int volume, Action? callback) {
    if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(outputDevice) || volume < 0 || volume > 100) {
      return;
    }

    _soundsToPlay.Add(new SoundPlayingWrapper(filename, outputDevice, volume, callback));
  }

  /// <summary>
  ///   The thread that manages playing the sounds.
  /// </summary>
  private void SoundPlayThreadMain() {
    while (true) {
      SoundPlayingWrapper sound = _soundsToPlay.Take();
      if (sound.Equals(_exitSentinel)) {
        return;
      }

      if (string.IsNullOrWhiteSpace(sound.Filename) || !File.Exists(sound.Filename)) {
        continue;
      }

      try {
        using (var reader = new NAudioUtilities.AudioFileReader(sound.Filename))
        using (var soundOutputEvent = new WaveOutEvent())
        using (var signal = new ManualResetEventSlim(false)) {
          soundOutputEvent.DeviceNumber = NAudioUtilities.GetOutputDeviceIndex(sound.OutputDevice);
          soundOutputEvent.Volume = sound.Volume / 100f;
          soundOutputEvent.Init(reader);
          soundOutputEvent.PlaybackStopped += (_, _) => signal.Set();
          CurrentlyPlayingSound = true;
          soundOutputEvent.Play();
          signal.Wait();
          CurrentlyPlayingSound = false;
        }

        if (null != sound.Callback) {
          Task.Run(sound.Callback);
        }
      }
      catch (Exception) {
      }
    }
  }

  /// <summary>
  ///   A wrapper that contains all of the information we need to play a sound.
  /// </summary>
  public class SoundPlayingWrapper {
    /// <summary>
    ///   The callback to invoke after the sound is played.
    /// </summary>
    public Action? Callback;

    /// <summary>
    ///   The name of the file.
    /// </summary>
    public string Filename;

    /// <summary>
    ///   The output device to stream to.
    /// </summary>
    public string OutputDevice;

    /// <summary>
    ///   The volume to play the file at.
    /// </summary>
    public int Volume;

    /// <summary>
    ///   Initializes a new instance of the <see cref="SoundPlayingWrapper" /> class.
    /// </summary>
    /// <param name="filename">The name of the file.</param>
    /// <param name="outputDevice">The output device to stream to.</param>
    /// <param name="volume">The volume to play the file at.</param>
    /// <param name="callback">The callback to invoke after the sound is played.</param>
    public SoundPlayingWrapper(string filename, string outputDevice, int volume, Action? callback) {
      Filename = filename;
      OutputDevice = outputDevice;
      Volume = volume;
      Callback = callback;
    }
  }
}