using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Nullinside.Api.Common.Twitch;
using Nullinside.TwitchStreamingTools.Models;
using Nullinside.TwitchStreamingTools.Utilities;

using ReactiveUI;

using TwitchLib.Client.Events;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles settings up and viewing chat.
/// </summary>
public partial class ChatViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   The application configuration;
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The twitch chat log.
  /// </summary>
  private readonly ITwitchChatLog _twitchChatLog;

  /// <summary>
  ///   The twitch chat client.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   The list of chat names selected in the list.
  /// </summary>
  [ObservableProperty] private ObservableCollection<string> _selectedTwitchChatNames = [];

  /// <summary>
  ///   The current position of the cursor for the text box showing our chat logs, increment to move down.
  /// </summary>
  [ObservableProperty] private int _textBoxCursorPosition;

  /// <summary>
  ///   The current twitch chat.
  /// </summary>
  [ObservableProperty] private string? _twitchChat = string.Empty;

  /// <summary>
  ///   The current twitch chat name entered by the user.
  /// </summary>
  [ObservableProperty] private string? _twitchChatName;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ChatViewModel" /> class.
  /// </summary>
  /// <param name="twitchClient">The twitch chat client.</param>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchChatLog">The twitch chat log.</param>
  public ChatViewModel(ITwitchClientProxy twitchClient, IConfiguration configuration, ITwitchChatLog twitchChatLog) {
    _twitchChatLog = twitchChatLog;
    _twitchClient = twitchClient;
    _configuration = configuration;

    OnAddChat = ReactiveCommand.Create(OnAddChatCommand);
    OnRemoveChat = ReactiveCommand.Create<string>(OnRemoveChatCommand);

    Application.Current!.TryFindResource("DeleteRegular", out object? icon);
    DeleteIcon = (StreamGeometry)icon!;
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "ChatRegular";

  /// <summary>
  ///   The delete icon graphic for the UI.
  /// </summary>
  public StreamGeometry DeleteIcon { get; set; }

  /// <summary>
  ///   Called when adding a chat.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnAddChat { get; }

  /// <summary>
  ///   Called when removing a chat.
  /// </summary>
  public ReactiveCommand<string, Unit> OnRemoveChat { get; }

  /// <inheritdoc />
  public void Dispose() {
    foreach (TwitchChatConfiguration channel in _configuration.TwitchChats ?? []) {
      if (string.IsNullOrWhiteSpace(channel.TwitchChannel)) {
        continue;
      }

      _twitchClient.RemoveMessageCallback(channel.TwitchChannel, OnChatMessage);
    }

    OnAddChat.Dispose();
    OnRemoveChat.Dispose();
  }

  /// <summary>
  ///   Handles adding a new chat to monitor.
  /// </summary>
  private void OnAddChatCommand() {
    // Ensure we have a username
    string? username = TwitchChatName?.Trim();
    if (string.IsNullOrWhiteSpace(username) || SelectedTwitchChatNames.Contains(username)) {
      return;
    }

    SelectedTwitchChatNames.Add(username);

    // Add the callback, we don't need to wait for it to return it can run in the background.
    _ = _twitchClient.AddMessageCallback(username, OnChatMessage);

    // Clear out the textbox so the user knows we took their input
    TwitchChatName = null;

    // Update and write the configuration
    _configuration.TwitchChats = (
      from user in SelectedTwitchChatNames
      select new TwitchChatConfiguration {
        TwitchChannel = user,
        OutputDevice = Configuration.GetDefaultAudioDevice(),
        TtsOn = true,
        TtsVoice = Configuration.GetDefaultTtsVoice(),
        TtsVolume = Configuration.GetDefaultTtsVolume() ?? 50u
      }).ToList();

    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Handles removing a chat we were monitoring.
  /// </summary>
  /// <param name="channel">The chat to remove.</param>
  private void OnRemoveChatCommand(string channel) {
    SelectedTwitchChatNames.Remove(channel);

    // Remove the callback
    _twitchClient.RemoveMessageCallback(channel, OnChatMessage);

    // Update and write the configuration
    _configuration.TwitchChats = (
      from user in SelectedTwitchChatNames
      select new TwitchChatConfiguration {
        TwitchChannel = user,
        OutputDevice = Configuration.GetDefaultAudioDevice(),
        TtsOn = true,
        TtsVoice = Configuration.GetDefaultTtsVoice(),
        TtsVolume = Configuration.GetDefaultTtsVolume() ?? 50u
      }).ToList();

    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Handles a new chat message being received from <see cref="_twitchClient" />.
  /// </summary>
  /// <param name="msg">The message received.</param>
  private void OnChatMessage(OnMessageReceivedArgs msg) {
    if (SelectedTwitchChatNames.Count > 1) {
      TwitchChat = (TwitchChat + FormatChatMessage(msg.ChatMessage.Username, msg.ChatMessage.Message, msg.GetTimestamp() ?? DateTime.UtcNow, msg.ChatMessage.Channel)).Trim();
    }
    else {
      TwitchChat = (TwitchChat + FormatChatMessage(msg.ChatMessage.Username, msg.ChatMessage.Message, msg.GetTimestamp() ?? DateTime.UtcNow)).Trim();
    }

    TwitchChat += "\n";
    TextBoxCursorPosition = int.MaxValue;
  }

  /// <summary>
  ///   Formats a chat message for display.
  /// </summary>
  /// <param name="username">The username of the user that sent the message.</param>
  /// <param name="message">The chat message sent by the user.</param>
  /// <param name="timestamp">The timestamp of the message, in UTC.</param>
  /// <param name="channel">The channel the message was sent in, if you want it included.</param>
  /// <returns>The formatted message.</returns>
  private string FormatChatMessage(string username, string message, DateTime timestamp, string? channel = null) {
    return null == channel ? $"[{timestamp:MM/dd/yy H:mm:ss}] {username}: {message}" : $"[{timestamp:MM/dd/yy H:mm:ss}] ({channel}) {username}: {message}";
  }

  /// <summary>
  ///   Handles registering for twitch chat messages while the UI is open.
  /// </summary>
  public override void OnLoaded() {
    base.OnLoaded();

    // Connect to the twitch chats from the configuration.
    InitializeTwitchChatConnections();

    // Loads the chat history from the log file.
    PopulateChatHistory();

    // Move the textbox to the end
    TextBoxCursorPosition = int.MaxValue;
  }

  /// <summary>
  ///   Populates the UI with the historic list of chat messages.
  /// </summary>
  private void PopulateChatHistory() {
    // Get the history of messages
    IEnumerable<TwitchChatMessage> messages = _twitchChatLog.GetMessages();

    // Convert them into a string
    var sb = new StringBuilder();
    sb.AppendJoin('\n', messages.Select(l => FormatChatMessage(l.Username, l.Message, l.Timestamp, l.Channel)));
    sb.Append('\n');

    // Update the UI
    TwitchChat = sb.ToString();

    // Move the textbox to the end
    TextBoxCursorPosition = int.MaxValue;
  }

  /// <summary>
  ///   Connects to the twitch chats in the configuration file.
  /// </summary>
  private void InitializeTwitchChatConnections() {
    if (Design.IsDesignMode) {
      return;
    }

    foreach (TwitchChatConfiguration channel in _configuration.TwitchChats ?? []) {
      if (string.IsNullOrWhiteSpace(channel.TwitchChannel)) {
        continue;
      }

      SelectedTwitchChatNames.Add(channel.TwitchChannel);
      Task.Run(() => _twitchClient.AddMessageCallback(channel.TwitchChannel, OnChatMessage));
    }
  }

  /// <summary>
  ///   Handles unregistering for twitch chat messages while the UI is closed.
  /// </summary>
  public override void OnUnloaded() {
    base.OnUnloaded();
    Dispose();
  }
}