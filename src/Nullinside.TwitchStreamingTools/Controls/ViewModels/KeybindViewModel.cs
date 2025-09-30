using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Nullinside.TwitchStreamingTools.Models;
using Nullinside.TwitchStreamingTools.Services;

namespace Nullinside.TwitchStreamingTools.Controls.ViewModels;

/// <summary>
///   Handles storing information required to visualize a keybind.
/// </summary>
public partial class KeybindViewModel : ObservableObject {
  /// <summary>
  ///   The listener for keystrokes on the keyboard.
  /// </summary>
  private readonly IGlobalKeyPressService _service;

  /// <summary>
  ///   The keybind, if set.
  /// </summary>
  [ObservableProperty] private Keybind? _keybind;

  /// <summary>
  ///   True if listening for keystrokes, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _listening;

  /// <summary>
  ///   Initializes a new instance of the <see cref="KeybindViewModel" /> class.
  /// </summary>
  /// <param name="service">The listener for keystrokes on the keyboard.</param>
  public KeybindViewModel(IGlobalKeyPressService service) {
    _service = service;
  }

  /// <summary>
  ///   Starts listening for keystrokes.
  /// </summary>
  [RelayCommand]
  private void StartListenKeystroke() {
    Listening = true;
    _service.OnKeystroke -= OnKeystroke;
    _service.OnKeystroke += OnKeystroke;
  }

  /// <summary>
  ///   Called whenever a keystroke is pressed.
  /// </summary>
  /// <param name="keybind">The key that was press.</param>
  private void OnKeystroke(Keybind keybind) {
    if (_service.IsModifier(keybind.Key)) {
      return;
    }

    Keybind = keybind.Key == Keys.Escape ? null : keybind;
    _service.OnKeystroke -= OnKeystroke;
    Listening = false;
  }
}