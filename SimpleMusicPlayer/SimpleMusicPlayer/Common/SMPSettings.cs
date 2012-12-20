using Newtonsoft.Json;

namespace SimpleMusicPlayer.Common
{
  public class SMPSettings
  {
    [JsonIgnore] public const string SettingsFile = "settings.json";

    public MainSettings MainSettings { get; set; }
    public PlayerSettings PlayerSettings { get; set; }

    public static SMPSettings GetEmptySettings() {
      return new SMPSettings() { MainSettings = new MainSettings(), PlayerSettings = new PlayerSettings() };
    }
  }

  public class MainSettings
  {
  }

  public class PlayerSettings
  {
    public PlayerSettings() {
      this.Volume = 1;
    }

    public float Volume { get; set; }
  }
}