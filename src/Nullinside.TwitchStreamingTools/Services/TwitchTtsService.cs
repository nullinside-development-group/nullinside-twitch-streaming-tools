using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Avalonia.Controls;

using log4net;

using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Tts;
using Nullinside.TwitchStreamingTools.Utilities;

namespace Nullinside.TwitchStreamingTools.Services;

/// <summary>
///   Connects twitch chats to the TTS player whenever the configuration is updated.
/// </summary>
public class TwitchTtsService : ITwitchTtsService {
  /// <summary>
  ///   The length of time to wait between polls with nothing to do.
  /// </summary>
  private const int POLL_MILLISECONDS = 500;

  /// <summary>
  ///   The fully configured objects responsible for reading TTS chats.
  /// </summary>
  private readonly IList<TwitchChatTts> _chats = new List<TwitchChatTts>();

  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The service that listens for keystrokes.
  /// </summary>
  private readonly IGlobalKeyPressService _keyPressService;

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(TwitchTtsService));

  /// <summary>
  ///   The thread that polls for updates to the configuration file.
  /// </summary>
  private readonly Thread _thread;

  /// <summary>
  ///   The twitch chat log.
  /// </summary>
  private readonly ITwitchChatLog _twitchChatLog;

  /// <summary>
  ///   The twitch chat client to forward chat messages from.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClientProxy;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchTtsService" /> class.
  /// </summary>
  /// <param name="twitchClientProxy">The twitch chat client.</param>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchChatLog">The twitch chat log.</param>
  /// <param name="keyPressService">The service that listens for keystrokes.</param>
  public TwitchTtsService(ITwitchClientProxy twitchClientProxy, IConfiguration configuration, ITwitchChatLog twitchChatLog, IGlobalKeyPressService keyPressService) {
    _keyPressService = keyPressService;
    _keyPressService.OnKeystroke += OnKeystroke;
    _configuration = configuration;
    _twitchClientProxy = twitchClientProxy;
    _twitchChatLog = twitchChatLog;
    _thread = new Thread(Main) {
      IsBackground = true
    };

    _thread.Start();
  }

  /// <summary>
  ///   Handles listening for keystrokes to update the TTS queue.
  /// </summary>
  /// <param name="keybind">The keystroke.</param>
  private void OnKeystroke(Keybind keybind) {
    Keybind? skip = _configuration.SkipTtsKey;
    if (null != skip) {
      if (keybind.Key == skip.Key &&
          keybind.IsAlt == skip.IsAlt &&
          keybind.IsCtrl == skip.IsCtrl &&
          keybind.IsShift == skip.IsShift) {
        List<TwitchChatTts> chats = _chats.ToList();
        foreach (TwitchChatTts chat in chats) {
          try {
            chat.SkipCurrentTts();
          }
          catch {
            // Do nothing, just try to skip the best we can.
          }
        }
      }
    }

    Keybind? skipAll = _configuration.SkipAllTtsKey;
    if (null != skipAll) {
      if (keybind.Key == skipAll.Key &&
          keybind.IsAlt == skipAll.IsAlt &&
          keybind.IsCtrl == skipAll.IsCtrl &&
          keybind.IsShift == skipAll.IsShift) {
        List<TwitchChatTts> chats = _chats.ToList();
        foreach (TwitchChatTts chat in chats) {
          try {
            chat.SkipAllTts();
          }
          catch {
            // Do nothing, just try to skip the best we can.
          }
        }
      }
    }
  }

  /// <summary>
  ///   The main loop for this service.
  /// </summary>
  private void Main() {
    do {
      try {
        // Any chat we're currently connected to that isn't in the configuration file should be disconnected from.
        DisconnectChatsNotInConfig();

        // Any chat we're not currently connected to we should be.
        ConnectChatsInConfig();

        // Wait for a bit before checking again.
        Thread.Sleep(POLL_MILLISECONDS);
      }
      catch (Exception ex) {
        _logger.Error("Failed a TTS loop", ex);
      }
    } while (true);
  }

  /// <summary>
  ///   Connects to any configuration found in the config file that we are not currently connected to.
  /// </summary>
  private void ConnectChatsInConfig() {
    if (Design.IsDesignMode) {
      return;
    }

    List<string?>? missing = _configuration.TwitchChats?
      .Select(c => c.TwitchChannel)
      .Except(_chats?.Select(c => c.ChatConfig?.TwitchChannel) ?? [])
      .Where(i => !string.IsNullOrWhiteSpace(i))
      .ToList();

    if (null == missing || missing.Count == 0) {
      return;
    }

    foreach (string? newChat in missing) {
      var tts = new TwitchChatTts(_configuration, _twitchClientProxy, _configuration.TwitchChats?.FirstOrDefault(t => t.TwitchChannel == newChat), _twitchChatLog);
      tts.Connect();
      _chats?.Add(tts);
    }
  }

  /// <summary>
  ///   Disconnects the twitch chats no longer in the configuration.
  /// </summary>
  private void DisconnectChatsNotInConfig() {
    List<string?>? chatsNotInConfig = _chats?
      .Select(c => c.ChatConfig?.TwitchChannel)
      .Except(_configuration.TwitchChats?.Select(c => c?.TwitchChannel) ?? [])
      .Where(i => !string.IsNullOrWhiteSpace(i))
      .ToList();

    if (null == chatsNotInConfig || chatsNotInConfig.Count <= 0) {
      return;
    }

    foreach (string? disconnect in chatsNotInConfig) {
      TwitchChatTts? chat = _chats?.FirstOrDefault(c => c.ChatConfig?.TwitchChannel == disconnect);
      if (null == chat) {
        continue;
      }

      chat.Dispose();
      _chats?.Remove(chat);
    }
  }
}