using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Nullinside.TwitchStreamingTools.Models;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages.SettingsView;

/// <summary>
///   The view responsible for maintaining the list of phonetic words.
/// </summary>
public class TtsPhoneticWordsViewModel : ViewModelBase {
  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The reference to the word we're currently editing, null if we are not editing.
  /// </summary>
  private PhoneticWord? _editingPhonetic;

  /// <summary>
  ///   The user entered phonetic pronunciation of the word.
  /// </summary>
  private string? _userEnteredPhonetic;

  /// <summary>
  ///   The user entered word to pronounce phonetically.
  /// </summary>
  private string? _userEnteredWord;

  /// <summary>
  ///   The collection of all phonetic words.
  /// </summary>
  private ObservableCollection<PhoneticWord> _wordsToPhonetics = new();

  /// <summary>
  ///   Initializes a new instance of the <see cref="TtsPhoneticWordsViewModel" /> class.
  /// </summary>
  public TtsPhoneticWordsViewModel(IConfiguration configuration) {
    _configuration = configuration;
    if (null != _configuration.TtsPhonetics) {
      foreach (KeyValuePair<string, string> pair in _configuration.TtsPhonetics) {
        _wordsToPhonetics.Add(new PhoneticWord(this, pair.Key, pair.Value));
      }
    }
    else {
      _configuration.TtsPhonetics = new Dictionary<string, string>();
    }
  }

  /// <summary>
  ///   Gets or sets the user entered phonetic pronunciation of the word.
  /// </summary>
  public string? UserEnteredPhonetic {
    get => _userEnteredPhonetic;
    set => this.RaiseAndSetIfChanged(ref _userEnteredPhonetic, value);
  }

  /// <summary>
  ///   Gets or sets the user entered word to pronounce phonetically.
  /// </summary>
  public string? UserEnteredWord {
    get => _userEnteredWord;
    set => this.RaiseAndSetIfChanged(ref _userEnteredWord, value);
  }

  /// <summary>
  ///   Gets or sets the list of words/usernames and their phonetic pronunciations.
  /// </summary>
  public ObservableCollection<PhoneticWord> WordsToPhonetics {
    get => _wordsToPhonetics;
    set => this.RaiseAndSetIfChanged(ref _wordsToPhonetics, value);
  }

  /// <summary>
  ///   Cancels the editing of the current word.
  /// </summary>
  public void CancelEntry() {
    _editingPhonetic = null;
    UserEnteredWord = "";
    UserEnteredPhonetic = "";
  }

  /// <summary>
  ///   Deletes the word from the list.
  /// </summary>
  /// <param name="word">The word to delete.</param>
  public void DeletePhonetic(string word) {
    if (string.IsNullOrWhiteSpace(word)) {
      return;
    }

    PhoneticWord? entry = _wordsToPhonetics.FirstOrDefault(w => word.Equals(w.Word));
    if (null == entry) {
      return;
    }

    _wordsToPhonetics.Remove(entry);
    RemoveFromConfig(word);
  }

  /// <summary>
  ///   Edits an existing word.
  /// </summary>
  /// <param name="word">The word to edit.</param>
  public void EditPhonetic(string word) {
    if (string.IsNullOrWhiteSpace(word)) {
      return;
    }

    PhoneticWord? entry = _wordsToPhonetics.FirstOrDefault(w => word.Equals(w.Word));
    if (null == entry) {
      return;
    }

    _editingPhonetic = entry;
    UserEnteredWord = entry.Word;
    UserEnteredPhonetic = entry.Phonetic;
  }

  /// <summary>
  ///   Saves the current word.
  /// </summary>
  public void SaveEntry() {
    if (string.IsNullOrWhiteSpace(UserEnteredWord) || string.IsNullOrWhiteSpace(UserEnteredPhonetic)) {
      return;
    }

    // If we are not currently editing.
    if (null == _editingPhonetic) {
      // If the word already exists in the list and this would be a duplicate then change the existing word.
      PhoneticWord? existing = _wordsToPhonetics.FirstOrDefault(w => UserEnteredWord.Equals(w.Word, StringComparison.InvariantCultureIgnoreCase));
      if (null != existing) {
        existing.Word = UserEnteredWord;
        existing.Phonetic = UserEnteredPhonetic;
      }
      else {
        // Otherwise, make a new word.
        _wordsToPhonetics.Add(new PhoneticWord(this, UserEnteredWord, UserEnteredPhonetic));
      }
    }
    else {
      // If we are currently editing, update the existing word.
      _editingPhonetic.Word = UserEnteredWord;
      _editingPhonetic.Phonetic = UserEnteredPhonetic;
    }

    // The collection that holds the words contains immutable objects so we need to remove it and add it back in.
    RemoveFromConfig(UserEnteredWord);
    _configuration.TtsPhonetics?.Add(new KeyValuePair<string, string>(UserEnteredWord, UserEnteredPhonetic));
    _editingPhonetic = null;
    UserEnteredWord = "";
    UserEnteredPhonetic = "";
    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Removes a word from the configuration.
  /// </summary>
  /// <param name="word">The word to remove.</param>
  private void RemoveFromConfig(string word) {
    if (string.IsNullOrWhiteSpace(word)) {
      return;
    }

    KeyValuePair<string, string>? existing = _configuration.TtsPhonetics?.FirstOrDefault(u => word.Equals(u.Key, StringComparison.InvariantCultureIgnoreCase));
    if (null != existing && !default(KeyValuePair<string, string>).Equals(existing)) {
      _configuration.TtsPhonetics?.Remove(existing.Value);
      _configuration.WriteConfiguration();
    }
  }
}