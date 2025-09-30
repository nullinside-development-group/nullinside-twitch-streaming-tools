using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using Nullinside.TwitchStreamingTools.ViewModels.Pages;

using ReactiveUI;

using MenuItem = Nullinside.TwitchStreamingTools.Models.MenuItem;

namespace Nullinside.TwitchStreamingTools.ViewModels;

/// <summary>
///   The view model for the main UI.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase {
  /// <summary>
  ///   The dependency injection service provider.
  /// </summary>
  private readonly IServiceProvider _provider;

  /// <summary>
  ///   A flag indicating whether the menu is open.
  /// </summary>
  [ObservableProperty] private bool _isMenuOpen = true;

  /// <summary>
  ///   True if the application is updating, false otherwise.
  /// </summary>
  [ObservableProperty] private bool _isUpdating;

  /// <summary>
  ///   The open page.
  /// </summary>
  [ObservableProperty] private ViewModelBase _page;

  /// <summary>
  ///   The currently selected page.
  /// </summary>
  [ObservableProperty] private MenuItem _selectedMenuItem;

  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
  /// </summary>
  /// <param name="provider">The dependency injection service provider.</param>
  public MainWindowViewModel(IServiceProvider provider) {
    _provider = provider;
    OnToggleMenu = ReactiveCommand.Create(() => IsMenuOpen = !IsMenuOpen);
    _isUpdating = Environment.GetCommandLineArgs().Contains("--update");
    MenuItems = new ObservableCollection<MenuItem>();

    // Setup the left menu items.
    InitializeMenuItems();

    // Set the default page to the account page
    _selectedMenuItem = MenuItems.First(p => typeof(AccountViewModel).IsAssignableTo(p.ModelType));

    // Map the selected item changing to its event
    PropertyChanged += (_, e) => {
      if (nameof(SelectedMenuItem).Equals(e.PropertyName)) {
        OnSelectedMenuItemChanged();
      }
    };

    // Set the initial page
    _page = (_provider.GetRequiredService(typeof(AccountViewModel)) as AccountViewModel)!;
  }

  /// <summary>
  ///   The menu items.
  /// </summary>
  public ObservableCollection<MenuItem> MenuItems { get; set; }

  /// <summary>
  ///   Called when toggling the menu open and close.
  /// </summary>
  public ReactiveCommand<Unit, bool> OnToggleMenu { get; }

  /// <summary>
  ///   Initializes the menu items.
  /// </summary>
  private void InitializeMenuItems() {
    // Get all the PageViewModelBase views and map them to their PageViewModelBase
    List<MenuItem>? pages = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .Where(t => (t.FullName?.StartsWith("Nullinside.TwitchStreamingTools.ViewModels.Pages") ?? false) &&
                  typeof(PageViewModelBase).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
      .Select(t => new MenuItem(t, (_provider.GetRequiredService(t) as PageViewModelBase)!.IconResourceKey))
      .OrderBy(t => t.Label)
      .ToList();

    // Add the menu items for display
    MenuItems.AddRange(pages);
  }

  /// <summary>
  ///   Links the <see cref="Page" /> showing on the screen with changes to the <see cref="SelectedMenuItem" />.
  /// </summary>
  private void OnSelectedMenuItemChanged() {
    var viewModel = _provider.GetService(SelectedMenuItem.ModelType) as ViewModelBase;
    if (null == viewModel) {
      return;
    }

    Page = viewModel;
  }
}