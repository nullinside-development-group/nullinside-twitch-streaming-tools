using System;
using System.Runtime.InteropServices;

namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   The structure representing the key pressed.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct KeyboardLowLevelHookStruct {
  /// <summary>
  ///   The keystroke.
  /// </summary>
  public int vkCode;

  /// <summary>
  ///   The scan code.
  /// </summary>
  public int scanCode;

  /// <summary>
  ///   The flags.
  /// </summary>
  public int flags;

  /// <summary>
  ///   The time the key was pressed.
  /// </summary>
  public int time;

  /// <summary>
  ///   The extra info.
  /// </summary>
  public IntPtr dwExtraInfo;
}