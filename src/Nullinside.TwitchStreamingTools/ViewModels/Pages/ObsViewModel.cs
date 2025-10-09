using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Avalonia.Collections;
using Avalonia.Input;

using CommunityToolkit.Mvvm.ComponentModel;

using log4net;

using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using OBSWebsocketDotNet.Types;

namespace Nullinside.TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles publishing your obs information to the website.
/// </summary>
public partial class ObsViewModel : PageViewModelBase {
  /// <summary>
  ///   The configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(ObsViewModel));

  /// <summary>
  ///   <seealso cref="_obs" /> is connected.
  /// </summary>
  [ObservableProperty] private bool _isConnected;

  /// <summary>
  ///   The obs websocket client.
  /// </summary>
  private OBSWebsocket _obs;

  /// <summary>
  ///   The obs websocket server address.
  /// </summary>
  [ObservableProperty] private string _obsServerAddress = "localhost:4455";

  /// <summary>
  ///   The obs websocket server password.
  /// </summary>
  [ObservableProperty] private string? _obsServerPassword;
  
  /// <summary>
  /// The list of inputs with volumes.
  /// </summary>
  [ObservableProperty] private ObservableCollection<ObsAudioInput> _inputVolumes = new();

  /// <summary>
  ///   Initializes a new instance of the <see cref="ObsViewModel" /> class.
  /// </summary>
  /// <param name="configuration">The configuration.</param>
  public ObsViewModel(IConfiguration configuration) {
    _obs = new OBSWebsocket();
    _configuration = configuration;
    _obsServerAddress = configuration.ObsServerAddress ?? _obsServerAddress;
    _obsServerPassword = configuration.ObsServerPassword;
    PropertyChanged += OnPropertyChanged;
    ConnectToWebsocketServer();
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "ShareAndroidRegular";

  /// <summary>
  ///   Updates the configuration.
  /// </summary>
  /// <param name="_">Unused sender.</param>
  /// <param name="e">The details on the property that changed.</param>
  private void OnPropertyChanged(object? _, PropertyChangedEventArgs e) {
    switch (e.PropertyName) {
      case nameof(ObsServerAddress):
        _configuration.ObsServerAddress = ObsServerAddress;
        ConnectToWebsocketServer();
        break;
      case nameof(ObsServerPassword):
        _configuration.ObsServerPassword = ObsServerPassword;
        ConnectToWebsocketServer();
        break;
    }

    _configuration.WriteConfiguration();
  }

  /// <summary>
  ///   Attempt to connect to the OBS websocket server.
  /// </summary>
  private void ConnectToWebsocketServer() {
    if (_obs?.IsConnected ?? false) {
      _obs.Connected -= ObsOnConnected;
      _obs.Disconnected -= ObsOnDisconnected;
      _obs.Disconnect();
    }

    _obs = new OBSWebsocket();
    _obs.Connected += ObsOnConnected;
    _obs.Disconnected += ObsOnDisconnected;

    _obs.ConnectAsync($"ws://{ObsServerAddress}", ObsServerPassword);
  }

  /// <summary>
  ///   Keeps track of the connected state on the UI.
  /// </summary>
  /// <param name="_">Unused sender.</param>
  /// <param name="e">The event arguments.</param>
  private void ObsOnConnected(object? _, EventArgs e) {
    IsConnected = true;
    _logger.Info($"Connected: {ObsServerAddress}");

    var inputsWithAudio = new Dictionary<string, float>();
    var inputList = _obs.GetInputList();
    foreach (var item in inputList) {
      try {
        inputsWithAudio[item.InputName] = _obs.GetInputVolume(item.InputName).VolumeMul;
      }
      catch (ErrorResponseException) {
        // Do nothing, this is just a way to filter the input list to only things that have audio.
      }
    }
    
    InputVolumes = new ObservableCollection<ObsAudioInput>(inputsWithAudio.Select(i => new ObsAudioInput { Name = i.Key, Volume = i.Value }));
  }

  /// <summary>
  ///   Keeps track of the connected state on the UI.
  /// </summary>
  /// <param name="_">Unused sender.</param>
  /// <param name="e">The disconnected information.</param>
  private void ObsOnDisconnected(object? _, ObsDisconnectionInfo e) {
    IsConnected = false;
    string? information = e.WebsocketDisconnectionInfo.Exception?.Message;
    information ??= e.DisconnectReason;
    _logger.Info($"Disconnected/Failed to Connect: {ObsServerAddress} Reason: {information}");
  }
}