namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   Information on what kind of event it is.
/// </summary>
public enum KeyboardMessage {
  /// <summary>
  ///   The key was pressed down.
  /// </summary>
  KEY_DOWN = 0x100,

  /// <summary>
  ///   The key was released after being pressed down.
  /// </summary>
  KEY_UP = 0x101,

  /// <summary>
  ///   No idea.
  /// </summary>
  SYS_KEY_DOWN = 0x104,

  /// <summary>
  ///   Don't ask me.
  /// </summary>
  SYS_KEY_UP = 0x105
}