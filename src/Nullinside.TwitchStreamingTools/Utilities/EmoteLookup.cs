using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Nullinside.TwitchStreamingTools.Utilities;

/// <summary>
///   Helper methods for looking up the emotes available on a channel.
/// </summary>
public static class EmoteLookup {
  /// <summary>
  ///   The cache of Better TTV emotes for each channel.
  /// </summary>
  private static readonly Dictionary<string, string[]> BETTER_TTV_CACHE = new();

  /// <summary>
  ///   The cache of FrankerzFace emotes for each channel.
  /// </summary>
  private static readonly Dictionary<string, string[]> FRANKERZ_FACE_CACHE = new();

  /// <summary>
  ///   Gets the FrankerzFace emotes for the channel.
  /// </summary>
  /// <param name="channel">The channel to look up the emotes for.</param>
  /// <returns>An enumerable of enabled emotes if found, an empty enumerable otherwise.</returns>
  public static IEnumerable<string> GetFrankerzFaceEmotes(string channel) {
    // Try to use the emotes in the cache first.
    if (FRANKERZ_FACE_CACHE.ContainsKey(channel)) {
      return FRANKERZ_FACE_CACHE[channel];
    }

    // Query the API for the list of shared emotes
    var client = new HttpClient();
    Task<HttpResponseMessage> httpRequest = client.GetAsync($"https://api.frankerfacez.com/v1/room/{channel}");
    httpRequest.Wait();
    Task<string> pageContent = httpRequest.Result.Content.ReadAsStringAsync();
    pageContent.Wait();
    JObject pageContentJson = JObject.Parse(pageContent.Result);

    FRANKERZ_FACE_CACHE[channel] = pageContentJson["sets"]?.FirstOrDefault()?.FirstOrDefault()?["emoticons"]?
      .Where(e => null != e["name"]?.Value<string>())
      // ReSharper disable once RedundantEnumerableCastCall
      .Select(e => e["name"]?.Value<string>()).Cast<string>().ToArray() ?? Enumerable.Empty<string>().ToArray();
    return FRANKERZ_FACE_CACHE[channel];
  }

  /// <summary>
  ///   Gets the Better TTV emotes for the channel.
  /// </summary>
  /// <param name="roomId">The numeric twitch ID of the channel.</param>
  /// <returns>An enumerable of enabled emotes if found, an empty enumerable otherwise.</returns>
  public static IEnumerable<string> GetBetterTtvEmotes(string roomId) {
    // Try to use the emotes in the cache first.
    if (BETTER_TTV_CACHE.ContainsKey(roomId)) {
      return BETTER_TTV_CACHE[roomId];
    }

    // Query the API for the list of personal and shared emotes
    var client = new HttpClient();
    Task<HttpResponseMessage> httpRequest = client.GetAsync($"https://api.betterttv.net/3/cached/users/twitch/{roomId}");
    httpRequest.Wait();
    Task<string> pageContent = httpRequest.Result.Content.ReadAsStringAsync();
    pageContent.Wait();
    JObject pageContentJson = JObject.Parse(pageContent.Result);
    IEnumerable<string?> channelEmotes = pageContentJson["channelEmotes"]?.Select(e => e["code"]?.Value<string>()) ?? Enumerable.Empty<string>();
    IEnumerable<string?> sharedEmotes = pageContentJson["sharedEmotes"]?.Select(e => e["code"]?.Value<string>()) ?? Enumerable.Empty<string>();

    // ReSharper disable once RedundantEnumerableCastCall
    BETTER_TTV_CACHE[roomId] = channelEmotes.Concat(sharedEmotes).Cast<string>().ToArray();
    return BETTER_TTV_CACHE[roomId];
  }
}