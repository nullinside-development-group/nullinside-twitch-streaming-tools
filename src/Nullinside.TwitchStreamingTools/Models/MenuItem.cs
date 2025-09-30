using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nullinside.TwitchStreamingTools.Models;

/// <summary>
///   Represents a menu item on the left navigation of the main UI.
/// </summary>
public class MenuItem {
  /// <summary>
  ///   Initializes a new instance of the <see cref="MenuItem" /> class.
  /// </summary>
  /// <param name="type">The view model to generate the page from.</param>
  /// <param name="iconKey">The key name of the icon to use.</param>
  public MenuItem(Type type, string iconKey) {
    ModelType = type;
    Label = type.Name.Replace("ViewModel", "");

    Application.Current!.TryFindResource(iconKey, out object? icon);
    Icon = (StreamGeometry)icon!;
  }

  /// <summary>
  ///   The display name on the screen.
  /// </summary>
  public string Label { get; set; }

  /// <summary>
  ///   The view model.
  /// </summary>
  public Type ModelType { get; set; }

  /// <summary>
  ///   The view model.
  /// </summary>
  public StreamGeometry Icon { get; set; }
}