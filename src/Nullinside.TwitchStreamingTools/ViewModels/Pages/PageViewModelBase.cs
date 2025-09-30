using System.Reactive;

using ReactiveUI;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   A base class for all pages of the application that are navigable through the left nav of the application.
/// </summary>
public abstract class PageViewModelBase : ViewModelBase {
  /// <summary>
  ///   Initializes a new instance of the <see cref="PageViewModelBase" /> class.
  /// </summary>
  protected PageViewModelBase() {
    OnLoadedCommand = ReactiveCommand.Create(OnLoaded);
    OnUnloadedCommand = ReactiveCommand.Create(OnUnloaded);
  }

  /// <summary>
  ///   The style resource key name of the icon.
  /// </summary>
  public abstract string IconResourceKey { get; }

  /// <summary>
  ///   Called when the UI is loaded.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnLoadedCommand { protected set; get; }

  /// <summary>
  ///   Called when the UI is unloaded.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnUnloadedCommand { protected set; get; }

  /// <summary>
  ///   Called then Ui is loaded.
  /// </summary>
  public virtual void OnLoaded() {
    // Just exist to be overridden.
  }

  /// <summary>
  ///   Called when the Ui is unloaded.
  /// </summary>
  public virtual void OnUnloaded() {
  }
}