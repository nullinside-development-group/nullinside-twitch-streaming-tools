using System;
using System.Collections.Generic;

using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools.Utilities;

/// <summary>
///   The contract for a logger that keeps old twitch messages.
/// </summary>
public interface ITwitchChatLog {
  /// <summary>
  ///   The maximum number of messages to keep in the log, anything larger than this number will be deleted starting with
  ///   the oldest messages.
  /// </summary>
  int MaximumMessageCount { get; set; }

  /// <summary>
  ///   The maximum message age before the message should be deleted, anything older than this will be deleted starting
  ///   with the oldest messages.
  /// </summary>
  TimeSpan MaximumMessageAge { get; set; }

  /// <summary>
  ///   Retrieves all messages.
  /// </summary>
  /// <param name="channel">If set, filter messages to those sent in a channel, otherwise all channels.</param>
  /// <returns>All known twitch messages.</returns>
  IEnumerable<TwitchChatMessage> GetMessages(string? channel = null);

  /// <summary>
  ///   Adds a twitch chat log.
  /// </summary>
  /// <param name="message">The chat messages.</param>
  void AddMessage(TwitchChatMessage message);
}