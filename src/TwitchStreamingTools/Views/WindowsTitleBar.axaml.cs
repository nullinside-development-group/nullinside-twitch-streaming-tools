﻿using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace TwitchStreamingTools.Views;

/// <summary>
///   The window menu bar at the top of the UI
/// </summary>
/// <remarks>From: https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate.git</remarks>
public partial class WindowsTitleBar : UserControl {
  /// <summary>
  ///   A flag indicating whether the system bar should be seemless with other content or have its own vertical space.
  /// </summary>
  public static readonly StyledProperty<bool> IS_SEAMLESS_PROPERTY =
    AvaloniaProperty.Register<WindowsTitleBar, bool>(nameof(IsSeamless));

  private readonly Button _closeButton;
  private readonly NativeMenuBar _defaultMenuBar;
  private readonly Button _maximizeButton;
  private readonly Path _maximizeIcon;
  private readonly ToolTip _maximizeToolTip;
  private readonly Button _minimizeButton;
  private readonly NativeMenuBar _seamlessMenuBar;
  private readonly TextBlock _systemChromeTitle;

  private readonly DockPanel _titleBar;
  private readonly DockPanel _titleBarBackground;
  private readonly Image _windowIcon;

  /// <summary>
  ///   Initializes a new instance of the <see cref="WindowsTitleBar" /> class.
  /// </summary>
  public WindowsTitleBar() {
    InitializeComponent();
    _minimizeButton = this.FindControl<Button>("MinimizeButton")!;
    _maximizeButton = this.FindControl<Button>("MaximizeButton")!;
    _maximizeIcon = this.FindControl<Path>("MaximizeIcon")!;
    _maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip")!;
    _closeButton = this.FindControl<Button>("CloseButton")!;
    _windowIcon = this.FindControl<Image>("WindowIcon")!;

    _minimizeButton.Click += MinimizeWindow!;
    _maximizeButton.Click += MaximizeWindow!;
    _closeButton.Click += CloseWindow!;
    _windowIcon.DoubleTapped += CloseWindow!;

    _titleBar = this.FindControl<DockPanel>("TitleBar")!;
    _titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground")!;
    _systemChromeTitle = this.FindControl<TextBlock>("SystemChromeTitle")!;
    _seamlessMenuBar = this.FindControl<NativeMenuBar>("SeamlessMenuBar")!;
    _defaultMenuBar = this.FindControl<NativeMenuBar>("DefaultMenuBar")!;

    SubscribeToWindowState();
  }

  /// <summary>
  ///   A flag indicating whether the system bar should be seemless with other content or have its own vertical space.
  /// </summary>
  public bool IsSeamless {
    get => GetValue(IS_SEAMLESS_PROPERTY);
    set {
      SetValue(IS_SEAMLESS_PROPERTY, value);
      if (_titleBarBackground != null &&
          _systemChromeTitle != null &&
          _seamlessMenuBar != null &&
          _defaultMenuBar != null) {
        _titleBarBackground.IsVisible = IsSeamless ? false : true;
        _systemChromeTitle.IsVisible = IsSeamless ? false : true;
        _seamlessMenuBar.IsVisible = IsSeamless ? true : false;
        _defaultMenuBar.IsVisible = IsSeamless ? false : true;

        if (IsSeamless == false) {
          _titleBar.Resources["SystemControlForegroundBaseHighBrush"] =
            new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
        }
      }
    }
  }

  private void CloseWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;
    hostWindow.Close();
  }

  private void MaximizeWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;

    if (hostWindow.WindowState == WindowState.Normal) {
      hostWindow.WindowState = WindowState.Maximized;
    }
    else {
      hostWindow.WindowState = WindowState.Normal;
    }
  }

  private void MinimizeWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;
    hostWindow.WindowState = WindowState.Minimized;
  }

  private async void SubscribeToWindowState() {
    var hostWindow = VisualRoot as Window;

    while (hostWindow == null) {
      hostWindow = VisualRoot as Window;
      await Task.Delay(50).ConfigureAwait(false);
    }

    hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s => {
      if (s != WindowState.Maximized) {
        _maximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
        hostWindow.Padding = new Thickness(0, 0, 0, 0);
        _maximizeToolTip.Content = "Maximize";
      }

      if (s == WindowState.Maximized) {
        _maximizeIcon.Data =
          Geometry.Parse(
            "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
        hostWindow.Padding = new Thickness(7, 7, 7, 7);
        _maximizeToolTip.Content = "Restore Down";

        // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
        /*hostWindow.Padding = new Thickness(
                hostWindow.OffScreenMargin.Left,
                hostWindow.OffScreenMargin.Top,
                hostWindow.OffScreenMargin.Right,
                hostWindow.OffScreenMargin.Bottom);*/
      }
    });
  }

  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}