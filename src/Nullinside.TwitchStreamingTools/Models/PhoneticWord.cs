using Nullinside.TwitchStreamingTools.ViewModels;
using Nullinside.TwitchStreamingTools.ViewModels.Pages.SettingsView;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   A representation of a word that needs to be pronounced phonetically.
/// </summary>
public class PhoneticWord : ViewModelBase {
  /// <summary>
  ///   The view model that owns this object.
  /// </summary>
  private readonly TtsPhoneticWordsViewModel _viewModel;

  /// <summary>
  ///   The phonetic pronunciation of the word.
  /// </summary>
  private string _phonetic;

  /// <summary>
  ///   The word to pronounce phonetically.
  /// </summary>
  private string _word;

  /// <summary>
  ///   Initializes a new instance of the <see cref="PhoneticWord" /> class.
  /// </summary>
  /// <param name="viewModel"> The view model that owns this object.</param>
  /// <param name="word">The word to pronounce phonetically.</param>
  /// <param name="phonetic">The phonetic pronunciation of the word.</param>
  public PhoneticWord(TtsPhoneticWordsViewModel viewModel, string word, string phonetic) {
    _viewModel = viewModel;
    _word = word;
    _phonetic = phonetic;
  }

  /// <summary>
  ///   Gets or sets the phonetic pronunciation of the word.
  /// </summary>
  public string Phonetic {
    get => _phonetic;
    set => this.RaiseAndSetIfChanged(ref _phonetic, value);
  }

  /// <summary>
  ///   Gets or sets the word to pronounce phonetically.
  /// </summary>
  public string Word {
    get => _word;
    set => this.RaiseAndSetIfChanged(ref _word, value);
  }

  /// <summary>
  ///   Deletes this word from the list.
  /// </summary>
  public void DeletePhonetic() {
    _viewModel.DeletePhonetic(_word);
  }

  /// <summary>
  ///   Edits this word in the list.
  /// </summary>
  public void EditPhonetic() {
    _viewModel.EditPhonetic(_word);
  }
}