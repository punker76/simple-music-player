using System.Collections.Generic;
using MahApps.Metro.Native;
using Newtonsoft.Json;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer.Common
{
  public class SMPSettings
  {
    [JsonIgnore] public const string SettingsFile = "settings.json";

    public MainSettings MainSettings { get; set; }
    public PlayerSettings PlayerSettings { get; set; }

    public static SMPSettings GetEmptySettings() {
      return new SMPSettings {MainSettings = new MainSettings(), PlayerSettings = new PlayerSettings()};
    }
  }

  public class MainSettings
  {
    public WINDOWPLACEMENT? Placement { get; set; }
  }

  public class PlayerSettings : ViewModelBase
  {
    private bool shuffleMode;
    private bool repeatMode;

    public PlayerSettings() {
      this.Volume = 1;
    }

    public float Volume { get; set; }

    public bool Mute { get; set; }

    public bool ShuffleMode {
      get { return this.shuffleMode; }
      set {
        if (Equals(value, this.shuffleMode)) {
          return;
        }
        this.shuffleMode = value;
        this.OnPropertyChanged(() => this.ShuffleMode);
      }
    }

    public bool RepeatMode {
      get { return this.repeatMode; }
      set {
        if (Equals(value, this.repeatMode)) {
          return;
        }
        this.repeatMode = value;
        this.OnPropertyChanged(() => this.RepeatMode);
      }
    }

    public EqualizerSettings EqualizerSettings { get; set; }
  }

  public class EqualizerSettings
  {
    public string Name { get; set; }
    public Dictionary<string, float> GainValues { get; set; }
    public bool IsEnabled { get; set; }
  }
}