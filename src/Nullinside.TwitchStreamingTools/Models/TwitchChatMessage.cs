using System;

namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   A message sent in twitch chat.
/// </summary>
public class TwitchChatMessage {
  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchChatMessage" />.
  /// </summary>
  /// <param name="channel">The channel to the message was sent in.</param>
  /// <param name="username">The username of the sender.</param>
  /// <param name="message">The chat message.</param>
  /// <param name="timestamp">The timestamp of when the message was sent.</param>
  public TwitchChatMessage(string channel, string username, string message, DateTime timestamp) {
    Channel = channel;
    Username = username;
    Message = message;
    Timestamp = timestamp;
  }

  /// <summary>
  ///   The channel to the message was sent in.
  /// </summary>
  public string Channel { get; set; }

  /// <summary>
  ///   The username of the sender.
  /// </summary>
  public string Username { get; set; }

  /// <summary>
  ///   The chat message.
  /// </summary>
  public string Message { get; set; }

  /// <summary>
  ///   The timestamp of when the message was sent.
  /// </summary>
  public DateTime Timestamp { get; set; }
}