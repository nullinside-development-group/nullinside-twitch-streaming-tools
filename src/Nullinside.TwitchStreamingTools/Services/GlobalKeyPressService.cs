using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using log4net;

using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;

using PInvoke;

namespace Nullinside.TwitchStreamingTools.Services;

/// <summary>
///   Detects when keys are pressed anywhere on the OS.
/// </summary>
public class GlobalKeyPressService : IGlobalKeyPressService {
  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOG = LogManager.GetLogger(typeof(GlobalKeyPressService));

  /// <summary>
  ///   The pointer to the hook we added.
  /// </summary>
  private static User32.SafeHookHandle? s_hook;

  /// <summary>
  ///   Maps each modifier key to whether or not it's being held down. This is kind overkill but it's at least accurate.
  /// </summary>
  private static readonly Dictionary<Keys, bool> holding = new() {
    { Keys.Control, false },
    { Keys.ControlKey, false },
    { Keys.LControlKey, false },
    { Keys.RControlKey, false },
    { Keys.Shift, false },
    { Keys.ShiftKey, false },
    { Keys.LShiftKey, false },
    { Keys.RShiftKey, false }
  };

  /// <summary>
  ///   The possible modifier keys for quick comparison at runtime.
  /// </summary>
  private static readonly HashSet<Keys> modifiers = [
    Keys.Control, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey,
    Keys.Shift, Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey
  ];

  /// <summary>
  ///   The keystroke callback.
  /// </summary>
  private static Action<Keybind>? s_onKeystroke;

  /// <summary>
  ///   The thread to execute on.
  /// </summary>
  private readonly Thread _thread;

  /// <summary>
  ///   Initializes a new instance of the <see cref="GlobalKeyPressService" /> class.
  /// </summary>
  public GlobalKeyPressService() {
    _thread = new Thread(Main) {
      IsBackground = true
    };

    _thread.Start();
  }

  /// <summary>
  ///   Gets or sets the callbacks to invoke when a keystroke is pressed.
  /// </summary>
  public Action<Keybind>? OnKeystroke {
    get => s_onKeystroke;
    set => s_onKeystroke = value;
  }

  /// <inheritdoc />
  public bool IsModifier(Keys key) {
    return modifiers.Contains(key);
  }

  /// <summary>
  ///   The main loop which registers for keystrokes on the system and flushes the message buffer.
  /// </summary>
  private void Main() {
    try {
      // Set a hook.
      s_hook = User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, KeystrokeCallback, IntPtr.Zero, 0);

      unsafe {
        // Buffer the messages.
        User32.MSG message;
        while (0 != User32.GetMessage(&message, IntPtr.Zero, 0, 0)) { }
      }
    }
    catch (Exception ex) {
      LOG.Error("Failed to subscribe to Win32 windows hook", ex);
    }
  }

  /// <summary>
  ///   The event invoked when a key is pressed.
  /// </summary>
  /// <param name="nCode">The code.</param>
  /// <param name="wParam">The <seealso cref="KeyboardMessage" />.</param>
  /// <param name="lParam">The <seealso cref="KeyboardLowLevelHookStruct" />.</param>
  /// <returns>The next hook that should be called.</returns>
  private static int KeystrokeCallback(int nCode, IntPtr wParam, IntPtr lParam) {
    var keyboardEvent = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
    var whatHappened = (KeyboardMessage)wParam;
    var key = (Keys)keyboardEvent.vkCode;

    if (whatHappened == KeyboardMessage.KEY_DOWN) {
      if (modifiers.Contains(key)) {
        holding[key] = true;
      }

      // You don't get a keyboard event for ALT, you have to check it this way.
      bool holdingAlt = (keyboardEvent.flags & 0b_0001_0000) != 0;
      bool holdingCtrl = holding[Keys.Control] || holding[Keys.ControlKey] || holding[Keys.LControlKey] || holding[Keys.RControlKey];
      bool holdingShift = holding[Keys.Shift] || holding[Keys.ShiftKey] || holding[Keys.LShiftKey] || holding[Keys.RShiftKey];
      var keybind = new Keybind {
        IsCtrl = holdingCtrl,
        IsShift = holdingShift,
        IsAlt = holdingAlt,
        Key = key
      };
      LogKey(keybind);

      Action<Keybind>? callbacks = s_onKeystroke;
      foreach (Delegate callback in callbacks?.GetInvocationList() ?? []) {
        try {
          callback.DynamicInvoke(keybind);
        }
        catch (Exception ex) {
          LOG.Error("Callback failed", ex);
        }
      }
    }
    else if (whatHappened == KeyboardMessage.KEY_UP) {
      if (modifiers.Contains(key)) {
        holding[key] = false;
      }
    }

    return User32.CallNextHookEx(s_hook?.DangerousGetHandle() ?? 0, nCode, wParam, lParam);
  }

  /// <summary>
  ///   Logs the keystroke for debugging.
  /// </summary>
  /// <param name="keybind">The key pressed.</param>
  private static void LogKey(Keybind keybind) {
    LOG.Debug($"Key pressed: {keybind}");
  }
}