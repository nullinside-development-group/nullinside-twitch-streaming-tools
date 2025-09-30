using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nullinside.TwitchStreamingTools.Views.Pages.SettingsView;

/// <summary>
///   A view for managing the list of usernames to skip in TTS.
/// </summary>
public partial class TtsSkipUsernamesControl : UserControl {
  /// <summary>
  ///   Initializes a new instance of the <see cref="TtsSkipUsernamesControl" /> class.
  /// </summary>
  public TtsSkipUsernamesControl() {
    InitializeComponent();
  }

  /// <summary>
  ///   Initializes the GUI components.
  /// </summary>
  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}