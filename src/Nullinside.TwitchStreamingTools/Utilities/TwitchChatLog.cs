using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using Nullinside.TwitchStreamingTools.Models;

namespace Nullinside.TwitchStreamingTools.Utilities;

/// <summary>
///   A logger that keeps old twitch messages.
/// </summary>
public class TwitchChatLog : ITwitchChatLog {
  /// <summary>
  ///   The location of the log file.
  /// </summary>
  private static readonly string FILE_LOCATION =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nullinside",
      "twitch-streaming-tools", "twitch-log.json");

  /// <summary>
  ///   The twitch chat messages collection.
  /// </summary>
  private readonly List<TwitchChatMessage> _messages = new();

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchChatLog" /> class.
  /// </summary>
  public TwitchChatLog() {
    _messages.AddRange(ReadLog() ?? []);
  }

  /// <inheritdoc />
  public int MaximumMessageCount { get; set; } = 250;

  /// <inheritdoc />
  public TimeSpan MaximumMessageAge { get; set; } = TimeSpan.FromDays(7);

  /// <inheritdoc />
  public IEnumerable<TwitchChatMessage> GetMessages(string? channel = null) {
    lock (_messages) {
      return channel == null
        ? _messages.ToList()
        : _messages.Where(m => m.Channel == channel).ToList();
    }
  }

  /// <inheritdoc />
  public void AddMessage(TwitchChatMessage message) {
    lock (_messages) {
      _messages.Add(message);

      DateTime currentTime = DateTime.Now;
      _messages.RemoveAll(m => currentTime - m.Timestamp > MaximumMessageAge);

      if (_messages.Count > MaximumMessageCount) {
        _messages.RemoveRange(0, _messages.Count - MaximumMessageCount);
      }
    }

    WriteLog();
  }

  /// <summary>
  ///   Reads the log from disk.
  /// </summary>
  /// <returns>The log if successful, null otherwise.</returns>
  private static IEnumerable<TwitchChatMessage>? ReadLog() {
    try {
      string json = File.ReadAllText(FILE_LOCATION);
      return JsonConvert.DeserializeObject<IEnumerable<TwitchChatMessage>>(json);
    }
    catch { return null; }
  }

  /// <summary>
  ///   Writes the log file to disk.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  private bool WriteLog() {
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(FILE_LOCATION)!);

      string json;
      lock (_messages) {
        json = JsonConvert.SerializeObject(_messages);
      }

      File.WriteAllText(FILE_LOCATION, json);
      return true;
    }
    catch {
      return false;
    }
  }
}