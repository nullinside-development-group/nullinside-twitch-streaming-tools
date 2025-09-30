using Avalonia.Controls;
using Avalonia.Interactivity;

using Nullinside.TwitchStreamingTools.ViewModels.Pages;

namespace Nullinside.TwitchStreamingTools.Views.Pages;

/// <summary>
///   The base class for the page view.
/// </summary>
public partial class PageViewBase : UserControl {
  /// <summary>
  ///   Initializes a new instance of the <see cref="PageViewBase" /> class.
  /// </summary>
  public PageViewBase() {
    InitializeComponent();
  }

  /// <summary>
  ///   Handles passing the Loaded event down to the view model.
  /// </summary>
  /// <param name="sender">The invoker.</param>
  /// <param name="e">The event arguments.</param>
  private void Control_OnLoaded(object? sender, RoutedEventArgs e) {
    var dataContext = (DataContext as PageViewModelBase)!;
    dataContext.OnLoaded();
  }

  /// <summary>
  ///   Handles passing the Unloaded event down to the view model.
  /// </summary>
  /// <param name="sender">The invoker.</param>
  /// <param name="e">The event arguments.</param>
  private void Control_OnUnloaded(object? sender, RoutedEventArgs e) {
    var dataContext = (DataContext as PageViewModelBase)!;
    dataContext.OnUnloaded();
  }
}