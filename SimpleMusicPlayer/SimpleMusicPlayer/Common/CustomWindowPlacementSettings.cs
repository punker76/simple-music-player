using MahApps.Metro.Controls;
using MahApps.Metro.Native;

namespace SimpleMusicPlayer.Common
{
  public class CustomWindowPlacementSettings : IWindowPlacementSettings
  {
    private readonly SMPSettings smpSettings;

    public CustomWindowPlacementSettings(SMPSettings settings) {
      this.smpSettings = settings;
    }

    public WINDOWPLACEMENT? Placement { get; set; }

    public void Reload() {
      this.Placement = this.smpSettings.MainSettings.Placement;
    }

    public void Save() {
      this.smpSettings.MainSettings.Placement = this.Placement;
    }
  }
}