using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nullinside.TwitchStreamingTools.Views.Pages.SettingsView;

/// <summary>
///   The view responsible for managing the phonetic spellings of words.
/// </summary>
public partial class TtsPhoneticWordsControl : UserControl {
  /// <summary>
  ///   Initializes a new instance of the <see cref="TtsPhoneticWordsControl" /> class.
  /// </summary>
  public TtsPhoneticWordsControl() {
    InitializeComponent();
  }

  /// <summary>
  ///   Initializes the GUI components.
  /// </summary>
  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}