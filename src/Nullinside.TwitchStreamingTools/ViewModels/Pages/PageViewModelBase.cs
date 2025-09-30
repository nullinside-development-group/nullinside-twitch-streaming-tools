using System.Reactive;

using CommunityToolkit.Mvvm.Input;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   A base class for all pages of the application that are navigable through the left nav of the application.
/// </summary>
public abstract partial class PageViewModelBase : ViewModelBase {
  /// <summary>
  ///   Initializes a new instance of the <see cref="PageViewModelBase" /> class.
  /// </summary>
  protected PageViewModelBase() {
  }

  /// <summary>
  ///   The style resource key name of the icon.
  /// </summary>
  public abstract string IconResourceKey { get; }

  /// <summary>
  ///   Called then Ui is loaded.
  /// </summary>
  [RelayCommand]
  public virtual void OnLoaded() {
    // Just exist to be overridden.
  }

  /// <summary>
  ///   Called when the Ui is unloaded.
  /// </summary>
  [RelayCommand]
  public virtual void OnUnloaded() {
  }
}