using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Nullinside.TwitchStreamingTools.ViewModels;

namespace Nullinside.TwitchStreamingTools.Controls.ViewModels;

/// <summary>
///   Handles maintaining two lists and moving items between them.
/// </summary>
public partial class TwoListViewModel : ViewModelBase {
  /// <summary>
  ///   The behavior to maintain when double clicking
  /// </summary>
  public enum DoubleClickBehavior {
    /// <summary>
    ///   Move items from the list you double clicked on to the other list when the items are double clicked.
    /// </summary>
    MOVE_TO_OTHER_LIST,

    /// <summary>
    ///   Delete items from the list when double clicked.
    /// </summary>
    DELETE_FROM_LIST
  }

  /// <summary>
  ///   The header on the left list.
  /// </summary>
  [ObservableProperty] private string? _leftHeader;

  /// <summary>
  ///   The collection of items in the left list.
  /// </summary>
  [ObservableProperty] private ObservableCollection<string> _leftList;

  /// <summary>
  ///   The method to call when an item in the left list is double clicked.
  /// </summary>
  [ObservableProperty] private Action<string?>? _onLeftDoubleClick;

  /// <summary>
  ///   The method to call when an item in the right list is double clicked.
  /// </summary>
  [ObservableProperty] private Action<string?>? _onRightDoubleClick;

  /// <summary>
  ///   The header on the right list.
  /// </summary>
  [ObservableProperty] private string? _rightHeader;

  /// <summary>
  ///   The collection of items in the right list.
  /// </summary>
  [ObservableProperty] private ObservableCollection<string> _rightList;

  /// <summary>
  ///   The behavior of how to handle double clicking on items in the right list.
  /// </summary>
  [ObservableProperty] private DoubleClickBehavior _rightListBehavior;

  /// <summary>
  ///   A value indicating whether the left list should be sorted.
  /// </summary>
  [ObservableProperty] private bool _sortLeftList;

  /// <summary>
  ///   A value indicating whether the right list should be sorted.
  /// </summary>
  [ObservableProperty] private bool _sortRightList;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwoListViewModel" /> class.
  /// </summary>
  public TwoListViewModel() {
    _leftList = new ObservableCollection<string>();
    _rightList = new ObservableCollection<string>();
    OnLeftDoubleClick += OnLeftDoubleClicked;
    OnRightDoubleClick += OnRightDoubleClicked;
  }

  /// <summary>
  ///   Adds an item to the left list.
  /// </summary>
  /// <param name="item">The item to add.</param>
  public void AddLeftList(string item) {
    if (string.IsNullOrWhiteSpace(item)) {
      return;
    }

    if (!SortLeftList) {
      LeftList.Add(item);
      return;
    }

    AddToList(LeftList, item);
  }

  /// <summary>
  ///   Adds an item to the right list.
  /// </summary>
  /// <param name="item">The item to add.</param>
  public void AddRightList(string item) {
    if (!SortRightList) {
      RightList.Add(item);
      return;
    }

    AddToList(RightList, item);
  }

  /// <summary>
  ///   Removes an item from the left list.
  /// </summary>
  /// <param name="item">The item to remove.</param>
  public void RemoveLeftList(string item) {
    LeftList.Remove(item);
  }

  /// <summary>
  ///   Removes an item from the right list.
  /// </summary>
  /// <param name="item">The item to remove.</param>
  public void RemoveRightList(string item) {
    RightList.Remove(item);
  }

  /// <summary>
  ///   Moves items from the left list to the right list.
  /// </summary>
  /// <param name="selectedItem">The item to move.</param>
  protected virtual void OnLeftDoubleClicked(string? selectedItem) {
    if (string.IsNullOrWhiteSpace(selectedItem)) {
      return;
    }

    LeftList.Remove(selectedItem);
    AddRightList(selectedItem);
  }

  /// <summary>
  ///   Moves items from the right list to the left list.
  /// </summary>
  /// <param name="selectedItem">The item to move.</param>
  protected virtual void OnRightDoubleClicked(string? selectedItem) {
    if (string.IsNullOrWhiteSpace(selectedItem)) {
      return;
    }

    RightList.Remove(selectedItem);

    if (DoubleClickBehavior.MOVE_TO_OTHER_LIST == RightListBehavior) {
      AddLeftList(selectedItem);
    }
  }

  /// <summary>
  ///   Adds an item to the provided collection.
  /// </summary>
  /// <param name="collection">The collection to add to.</param>
  /// <param name="item">The item to add.</param>
  private void AddToList(ObservableCollection<string> collection, string item) {
    if (string.IsNullOrWhiteSpace(item)) {
      return;
    }

    if (0 == collection.Count) {
      collection.Add(item);
      return;
    }

    for (int i = 0; i < collection.Count; i++) {
      int comp = string.Compare(item, collection[i], StringComparison.InvariantCultureIgnoreCase);
      if (0 == comp || 0 > comp) {
        collection.Insert(i, item);
        return;
      }
    }

    collection.Add(item);
  }
}