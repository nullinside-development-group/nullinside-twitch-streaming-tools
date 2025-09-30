using System.Reactive;

using Nullinside.TwitchStreamingTools.Services;
using Nullinside.TwitchStreamingTools.ViewModels;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.Controls.ViewModels;

/// <summary>
///   Handles storing information required to visualize a keybind.
/// </summary>
public class KeybindViewModel : ViewModelBase {
  /// <summary>
  ///   The listener for keystrokes on the keyboard.
  /// </summary>
  private readonly IGlobalKeyPressService _service;

  /// <summary>
  ///   The keybind, if set.
  /// </summary>
  private Keybind? _keybind;

  /// <summary>
  ///   Initializes a new instance of the <see cref="KeybindViewModel" /> class.
  /// </summary>
  /// <param name="service">The listener for keystrokes on the keyboard.</param>
  public KeybindViewModel(IGlobalKeyPressService service) {
    _service = service;
    ListenForKeystroke = ReactiveCommand.Create(StartListenKeystroke);
  }

  /// <summary>
  ///   The keybind.
  /// </summary>
  public Keybind? Keybind {
    get => _keybind;
    set => this.RaiseAndSetIfChanged(ref _keybind, value);
  }

  /// <summary>
  ///   Listens for keystrokes.
  /// </summary>
  public ReactiveCommand<Unit, Unit> ListenForKeystroke { get; }

  /// <summary>
  ///   Starts listening for keystrokes.
  /// </summary>
  private void StartListenKeystroke() {
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

    Keybind = keybind;
    _service.OnKeystroke -= OnKeystroke;
  }
}