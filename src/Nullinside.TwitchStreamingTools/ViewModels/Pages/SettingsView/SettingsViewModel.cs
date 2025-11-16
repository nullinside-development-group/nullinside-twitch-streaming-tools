using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Speech.Synthesis;

using CommunityToolkit.Mvvm.ComponentModel;

using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;
using Nullinside.TwitchStreamingTools.Utilities;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages.SettingsView;

/// <summary>
///   Handles binding your application settings.
/// </summary>
public partial class SettingsViewModel : PageViewModelBase {
  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   Don't use anti-alias filtering (gain speed, lose quality)
  /// </summary>
  private bool _antiAliasingOff;

  /// <summary>
  ///   Detect the BPM rate of sound and adjust tempo to meet 'n' BPMs. If value not specified, just detects the BPM rate.
  /// </summary>
  private int _bpm;

  /// <summary>
  ///   The list of possible output devices that exist on the machine.
  /// </summary>
  [ObservableProperty] private ObservableCollection<string> _outputDevices;

  /// <summary>
  ///   Change sound pitch by n semitones (-60 to +60 semitones)
  /// </summary>
  private int _pitch;

  /// <summary>
  ///   Use quicker tempo change algorithm (gain speed, lose quality)
  /// </summary>
  private bool _quick;

  /// <summary>
  ///   Change sound rate by n percents (-95 to +5000 %)
  /// </summary>
  private int _rate;

  /// <summary>
  ///   True if "username says message" should be used as the template for TTS messages, false to read just the message.
  /// </summary>
  private bool _sayUsernameWithMessage;

  /// <summary>
  ///   The selected output device to send TTS to.
  /// </summary>
  private string? _selectedOutputDevice;

  /// <summary>
  ///   The TTS voice selected to send TTS to.
  /// </summary>
  private string? _selectedTtsVoice;

  /// <summary>
  ///   True if the advanced Text-to-Speech (TTS) settings are displayed.
  /// </summary>
  [ObservableProperty] private bool _showAdvancedTts;

  /// <summary>
  ///   The keybind for skipping all TTS messages.
  /// </summary>
  [ObservableProperty] private KeybindViewModel _skipAllTtsKeyBinding;

  /// <summary>
  ///   The keybind for skipping TTS messages.
  /// </summary>
  [ObservableProperty] private KeybindViewModel _skipTtsKeyBinding;

  /// <summary>
  ///   The speed (as a multiplicative).
  /// </summary>
  private double _speed;

  /// <summary>
  ///   Change sound tempo by n percents (-95 to +5000 %)
  /// </summary>
  private int _tempo;

  /// <summary>
  ///   The view model for the phonetic words list.
  /// </summary>
  [ObservableProperty] private TtsPhoneticWordsViewModel _ttsPhoneticWordsViewModel;

  /// <summary>
  ///   The control responsible for managing the list of usernames to skip.
  /// </summary>
  [ObservableProperty] private TtsSkipUsernamesViewModel _ttsSkipUsernamesViewModel;

  /// <summary>
  ///   The list of installed TTS voices on the machine.
  /// </summary>
  [ObservableProperty] private ObservableCollection<string> _ttsVoices;

  /// <summary>
  ///   The volume to play the TTS messages at.
  /// </summary>
  /// <remarks>0 is silent, 100 is full volume.</remarks>
  private uint _ttsVolume;

  /// <summary>
  ///   Tune algorithm for speech processing (default is for music)
  /// </summary>
  private bool _turnOnSpeech;

  /// <summary>
  ///   Initializes a new instance of the <see cref="SettingsViewModel" /> class.
  /// </summary>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="ttsPhoneticWordsViewModel">The view model for the phonetic words list.</param>
  /// <param name="ttsSkipUsernamesViewModel">The control responsible for managing the list of usernames to skip.</param>
  /// <param name="keybindViewModel">The skip TTS keybind.</param>
  /// <param name="keybindAllViewModel">The skip all TTS keybind.</param>
  public SettingsViewModel(IConfiguration configuration, TtsPhoneticWordsViewModel ttsPhoneticWordsViewModel, TtsSkipUsernamesViewModel ttsSkipUsernamesViewModel, KeybindViewModel keybindViewModel, KeybindViewModel keybindAllViewModel) {
    _configuration = configuration;
    _ttsPhoneticWordsViewModel = ttsPhoneticWordsViewModel;
    _ttsSkipUsernamesViewModel = ttsSkipUsernamesViewModel;
    _configuration.SoundStretchArgs ??= new SoundStretchArgs();
    _tempo = _configuration.SoundStretchArgs.Tempo ?? 0;
    _pitch = _configuration.SoundStretchArgs.Pitch ?? 0;
    _rate = _configuration.SoundStretchArgs.Rate ?? 0;
    _bpm = _configuration.SoundStretchArgs.Bpm ?? 0;
    _quick = _configuration.SoundStretchArgs.Quick;
    _antiAliasingOff = _configuration.SoundStretchArgs.AntiAliasingOff;
    _turnOnSpeech = _configuration.SoundStretchArgs.TurnOnSpeech;
    _speed = (Tempo / 50.0) + 1.0;
    _sayUsernameWithMessage = _configuration.SayUsernameWithMessage;
    _skipTtsKeyBinding = keybindViewModel;
    _skipTtsKeyBinding.Keybind = _configuration.SkipTtsKey;
    _skipTtsKeyBinding.PropertyChanged += OnSkipTtsKeybindChanged;
    _skipAllTtsKeyBinding = keybindAllViewModel;
    _skipAllTtsKeyBinding.Keybind = _configuration.SkipAllTtsKey;
    _skipAllTtsKeyBinding.PropertyChanged += OnSkipAllTtsKeybindChanged;

    ToggleAdvancedTtsCommand = ReactiveCommand.Create(() => ShowAdvancedTts = !ShowAdvancedTts);

    // Get the list of output devices and set the default to either what we have in the configuration or the system 
    // default whichever is more appropriate.
    var outputDevices = new List<string>();
    for (int i = 0; i < NAudioUtilities.GetTotalOutputDevices(); i++) {
      outputDevices.Add(NAudioUtilities.GetOutputDevice(i).ProductName);
    }

    _outputDevices = new ObservableCollection<string>(outputDevices);
    _selectedOutputDevice = Configuration.GetDefaultAudioDevice();

    // Get the list of TTS voices and set the default to either what we have in the configuration or the system 
    // default whichever is more appropriate.
    _ttsVoices = new ObservableCollection<string>(GetFilteredTtsVoices());
    _selectedTtsVoice = Configuration.GetDefaultTtsVoice();

    // Get the volume and set the default to either what we have in the configuration or to half-volume. Why half-volume?
    // No one knows. I just figured I'd at least try not to blow anyone's eardrums out. I'm sure this decision will haunt
    // me one day.
    _ttsVolume = Configuration.GetDefaultTtsVolume() ?? 50u;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "SettingsRegular";

  /// <summary>
  ///   The selected output device that audio will be sent to.
  /// </summary>
  public string? SelectedOutputDevice {
    get => _selectedOutputDevice;
    set {
      SetProperty(ref _selectedOutputDevice, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.OutputDevice = value;
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   The selected TTS voice will be used.
  /// </summary>
  public string? SelectedTtsVoice {
    get => _selectedTtsVoice;
    set {
      SetProperty(ref _selectedTtsVoice, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.TtsVoice = value;
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   The volume to play the TTS messages at.
  /// </summary>
  /// <remarks>0 is silent, 100 is full volume.</remarks>
  public uint TtsVolume {
    get => _ttsVolume;
    set {
      SetProperty(ref _ttsVolume, value);

      // Go through each twitch chat and update their property
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        chat.TtsVolume = value > 0 ? value : 0;
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   Change sound tempo by n percents (-95 to +5000 %)
  /// </summary>
  public int Tempo {
    get => _tempo;
    set {
      SetProperty(ref _tempo, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.Tempo = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Change sound pitch by n semitones (-60 to +60 semitones)
  /// </summary>
  public int Pitch {
    get => _pitch;
    set {
      SetProperty(ref _pitch, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.Pitch = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Change sound rate by n percents (-95 to +5000 %)
  /// </summary>
  public int Rate {
    get => _rate;
    set {
      SetProperty(ref _rate, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.Rate = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Detect the BPM rate of sound and adjust tempo to meet 'n' BPMs. If value not specified, just detects the BPM rate.
  /// </summary>
  public int Bpm {
    get => _bpm;
    set {
      SetProperty(ref _bpm, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.Bpm = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Use quicker tempo change algorithm (gain speed, lose quality)
  /// </summary>
  public bool Quick {
    get => _quick;
    set {
      SetProperty(ref _quick, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.Quick = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Don't use anti-alias filtering (gain speed, lose quality)
  /// </summary>
  public bool AntiAliasingOff {
    get => _antiAliasingOff;
    set {
      SetProperty(ref _antiAliasingOff, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.AntiAliasingOff = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Tune algorithm for speech processing (default is for music)
  /// </summary>
  public bool TurnOnSpeech {
    get => _turnOnSpeech;
    set {
      SetProperty(ref _turnOnSpeech, value);
      if (_configuration.SoundStretchArgs != null) {
        _configuration.SoundStretchArgs.TurnOnSpeech = value;
        _configuration.WriteConfiguration();
      }
    }
  }

  /// <summary>
  ///   Called when the show advanced settings is clicked.
  /// </summary>
  public ReactiveCommand<Unit, bool> ToggleAdvancedTtsCommand { protected set; get; }

  /// <summary>
  ///   The speed to play the TTS
  /// </summary>
  public double Speed {
    get => _speed;
    set {
      SetProperty(ref _speed, value);

      // In terms of tempo, 50 is 100% faster (2x speed). Scaling that equation linearly, we get:
      double tempo = (value - 1.0) * 50;

      if (_configuration.SoundStretchArgs != null) {
        Tempo = (int)Math.Round(tempo);
      }

      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   True if "username says message" should be used as the template for TTS messages, false to read just the message.
  /// </summary>
  public bool SayUsernameWithMessage {
    get => _sayUsernameWithMessage;
    set {
      SetProperty(ref _sayUsernameWithMessage, value);

      _configuration.SayUsernameWithMessage = value;
      _configuration.WriteConfiguration();
    }
  }

  /// <summary>
  ///   Gets the filtered list of voices, removing duplicates.
  /// </summary>
  /// <returns>The list of voice names.</returns>
  private IEnumerable<string> GetFilteredTtsVoices() {
    const string duplicatePostfix = " Desktop";
    using var speech = new SpeechSynthesizer();
    IEnumerable<string> allVoices = speech.GetInstalledVoices().Select(v => v.VoiceInfo.Name);
    var unique = new HashSet<string>();
    foreach (string voice in allVoices) {
      // If the voice ends in the " Desktop" it means there could be a duplicate voice without that on the end
      // e.g:
      // Microsoft David Desktop
      // vs
      // Microsoft David
      // 
      // These both point to the same thing and we only need one of them. The tricky part is we can't be sure which one
      // we will encounter so we have to check if the other exists and make sure only one of the two end up in the list.
      if (voice.EndsWith(duplicatePostfix)) {
        // If a " Desktop" voice has a voice without it in the list already, then don't add it. Skip this one.
        if (unique.Contains(voice.Substring(0, voice.Length - duplicatePostfix.Length))) {
          continue;
        }
      }
      else {
        // If it's not a desktop voice, check if this voice has another voice already with the " Desktop" on the end.
        // If we find it, remove it and add this one instead.
        if (unique.Contains(voice +  duplicatePostfix)) {
          unique.Remove(voice + duplicatePostfix);
        }
      }
      
      unique.Add(voice);
    }

    // Sort them to be nice.
    return unique.OrderByDescending(s => s);
  }

  /// <summary>
  ///   Handles updating the configuration when the skip TTS keybind changes.
  /// </summary>
  /// <param name="_">Unused sender argument.</param>
  /// <param name="e">The arguments about the properties that changed.</param>
  private void OnSkipTtsKeybindChanged(object? _, PropertyChangedEventArgs e) {
    if (!nameof(SkipTtsKeyBinding.Keybind).Equals(e.PropertyName)) {
      return;
    }

    _configuration.SkipTtsKey = SkipTtsKeyBinding.Keybind;
    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Handles updating the configuration when the skip all TTS keybind changes.
  /// </summary>
  /// <param name="_">Unused sender argument.</param>
  /// <param name="e">The arguments about the properties that changed.</param>
  private void OnSkipAllTtsKeybindChanged(object? _, PropertyChangedEventArgs e) {
    if (!nameof(SkipAllTtsKeyBinding.Keybind).Equals(e.PropertyName)) {
      return;
    }

    _configuration.SkipAllTtsKey = SkipAllTtsKeyBinding.Keybind;
    _configuration.WriteConfiguration();
  }
}