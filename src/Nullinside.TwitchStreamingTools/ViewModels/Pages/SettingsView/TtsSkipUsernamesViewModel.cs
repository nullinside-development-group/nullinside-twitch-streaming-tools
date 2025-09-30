using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;

using log4net;

using Nullinside.TwitchStreamingTools.Controls.ViewModels;
using Nullinside.TwitchStreamingTools.Models;
using Nullinside.TwitchStreamingTools.Utilities;

using ReactiveUI;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

using static DynamicData.ListEx;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages.SettingsView;

/// <summary>
///   Handles managing the list of users to skip in TTS.
/// </summary>
public class TtsSkipUsernamesViewModel : ViewModelBase {
  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOG = LogManager.GetLogger(typeof(TtsSkipUsernamesViewModel));

  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The list of channels we shouldn't try to pull the chatters from because we don't have permission to do so.
  /// </summary>
  private readonly List<string> _doNotScanChat = new();

  /// <summary>
  ///   A timer that handles periodically refreshing the twitch chat viewer list.
  /// </summary>
  private readonly Timer _userListRefreshTimer;

  /// <summary>
  ///   The view model that handles the two column control.
  /// </summary>
  private TwoListViewModel _twoListViewModel;

  /// <summary>
  ///   The user entered username to add to the skip list.
  /// </summary>
  private string? _userToAdd;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TtsSkipUsernamesViewModel" /> class.
  /// </summary>
  public TtsSkipUsernamesViewModel(IConfiguration configuration) {
    _configuration = configuration;
    _userListRefreshTimer = new Timer(1);
    _userListRefreshTimer.AutoReset = false;
    _userListRefreshTimer.Elapsed += UserListRefreshTimer_OnElapsed!;

    _twoListViewModel = new TwoListViewModel { RightListBehavior = TwoListViewModel.DoubleClickBehavior.DELETE_FROM_LIST, SortLeftList = true, SortRightList = true };
    _twoListViewModel.LeftHeader = "All Users";
    _twoListViewModel.RightHeader = "Skipped Users";

    if (null == _configuration.TtsUsernamesToSkip) {
      _configuration.TtsUsernamesToSkip = Constants.TWITCH_DEFAULT_BOT_LIST.ToList();
      _configuration.WriteConfiguration();
    }

    foreach (string username in _configuration.TtsUsernamesToSkip) {
      TwoListViewModel.AddRightList(username);
    }

    TwoListViewModel.RightList.CollectionChanged += TtsSkipped_OnCollectionChanged;
    _userListRefreshTimer.Start();
  }

  /// <summary>
  ///   Gets or sets the view model that handles the two column control.
  /// </summary>
  public TwoListViewModel TwoListViewModel {
    get => _twoListViewModel;
    set => this.RaiseAndSetIfChanged(ref _twoListViewModel, value);
  }

  /// <summary>
  ///   Gets or sets the user entered username to add to the skip list.
  /// </summary>
  public string? UserToAdd {
    get => _userToAdd;
    set => this.RaiseAndSetIfChanged(ref _userToAdd, value);
  }

  /// <summary>
  ///   Handles adding the <see cref="UserToAdd" /> to the skip list.
  /// </summary>
  public void AddUser() {
    string? user = UserToAdd;
    UserToAdd = null;
    if (string.IsNullOrWhiteSpace(user) || TwoListViewModel.RightList.Contains(user)) {
      return;
    }

    TwoListViewModel.AddRightList(user.ToLowerInvariant());
  }

  /// <summary>
  ///   Raised when the usernames to skip collection is changed.
  /// </summary>
  /// <param name="sender">The collection that changed.</param>
  /// <param name="e">The event arguments.</param>
  private void TtsSkipped_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    IList<string>? current = _configuration.TtsUsernamesToSkip?.ToList();
    if (null == current) {
      return;
    }

    string[] onlyNew = TwoListViewModel.RightList.Except(current).ToArray();
    string[] onlyOld = current.Except(TwoListViewModel.RightList).ToArray();
    current.RemoveMany(onlyOld);
    current.AddRange(onlyNew);

    _configuration.TtsUsernamesToSkip = current;
    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Handles refreshing the user list.
  /// </summary>
  /// <param name="sender">The timer.</param>
  /// <param name="e">The event arguments.</param>
  private async void UserListRefreshTimer_OnElapsed(object sender, ElapsedEventArgs e) {
    try {
      var api = new TwitchApiWrapper();
      if (string.IsNullOrWhiteSpace(api.OAuth?.AccessToken)) {
        return;
      }

      User? loggedInUser = await api.GetUser().ConfigureAwait(false);

      var usernames = new HashSet<string>();
      foreach (TwitchChatConfiguration chat in _configuration.TwitchChats ?? []) {
        if (string.IsNullOrWhiteSpace(chat.TwitchChannel) || _doNotScanChat.Contains(chat.TwitchChannel)) {
          continue;
        }

        try {
          (string? id, string? username) userInfo = await api.GetUser(chat.TwitchChannel).ConfigureAwait(false);
          if (null == userInfo.id || null == loggedInUser?.Id) {
            continue;
          }

          IEnumerable<Chatter> chatters = await api.GetChannelUsers(userInfo.id, loggedInUser.Id).ConfigureAwait(false);
          foreach (Chatter chatter in chatters) {
            usernames.Add(chatter.UserName.ToLowerInvariant());
          }
        }
        catch (BadTokenException) {
          _doNotScanChat.Add(chat.TwitchChannel);
        }
        catch (Exception ex) {
          LOG.Error($"Failed to get chatters for {chat.TwitchChannel}:", ex);
        }
      }

      string[] onlyNew = usernames.Except(TwoListViewModel.LeftList).Except(TwoListViewModel.RightList).ToArray();
      string[] onlyOld = TwoListViewModel.LeftList.Except(usernames).ToArray();

      foreach (string oldItem in onlyOld) {
        TwoListViewModel.RemoveLeftList(oldItem);
      }

      foreach (string newItem in onlyNew) {
        TwoListViewModel.AddLeftList(newItem);
      }

      _userListRefreshTimer.Interval = 5000;
      _userListRefreshTimer.Start();
    }
    catch {
      // do nothing don't crash
    }
  }
}