using System;
using System.Text;

using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools.Controls.ViewModels;

/// <summary>
///   A keybind.
/// </summary>
public class Keybind {
  /// <summary>
  ///   The key pressed.
  /// </summary>
  public Keys Key { get; set; }

  /// <summary>
  ///   Indicates whether the Control (Ctrl) key is pressed as part of the keybind.
  /// </summary>
  public bool IsCtrl { get; set; }

  /// <summary>
  ///   Indicates whether the Shift key is pressed as part of the keybind.
  /// </summary>
  public bool IsShift { get; set; }

  /// <summary>
  ///   Indicates whether the Alt key is pressed as part of the keybind.
  /// </summary>
  public bool IsAlt { get; set; }

  /// <summary>
  ///   Returns a string representation of the keybind, including any modifier keys (Ctrl, Shift, Alt) and the key itself.
  /// </summary>
  /// <returns>
  ///   A string representing the current keybind in the format of "Ctrl + Shift + Alt + Key",
  ///   including only the applicable modifiers.
  /// </returns>
  public override string ToString() {
    var sb = new StringBuilder();
    if (IsCtrl) {
      sb.Append("Ctrl + ");
    }

    if (IsShift) {
      sb.Append("Shift + ");
    }

    if (IsAlt) {
      sb.Append("Alt + ");
    }

    sb.Append(Key);
    return sb.ToString();
  }

  /// <inheritdoc />
  public override bool Equals(object? obj) {
    if (obj is Keybind keybind) {
      return Key == keybind.Key &&
             IsCtrl == keybind.IsCtrl &&
             IsShift == keybind.IsShift &&
             IsAlt == keybind.IsAlt;
    }

    return base.Equals(obj);
  }

  /// <inheritdoc />
  public override int GetHashCode() {
    return HashCode.Combine(Key, IsCtrl, IsShift, IsAlt);
  }
}