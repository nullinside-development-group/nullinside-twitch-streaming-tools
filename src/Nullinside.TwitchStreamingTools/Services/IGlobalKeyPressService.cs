using System;

using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools.Services;

/// <summary>
///   The contract for a service that detects when keys are pressed anywhere on the OS.
/// </summary>
public interface IGlobalKeyPressService {
  /// <summary>
  ///   Gets or sets the callbacks to invoke when a keystroke is pressed.
  /// </summary>
  Action<Keybind>? OnKeystroke { get; set; }

  /// <summary>
  ///   True if the key press is a modifier key and not a real keystroke.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <returns>True if modifier, false otherwise.</returns>
  bool IsModifier(Keys key);
}