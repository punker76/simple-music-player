using MahApps.Metro.Controls;
using MahApps.Metro.Native;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class CustomWindowPlacementSettings : IWindowPlacementSettings
  {
    private readonly IWindowPlacementSetting windowPlacementSetting;

    public CustomWindowPlacementSettings(IWindowPlacementSetting wps) {
      this.windowPlacementSetting = wps;
    }

    public WINDOWPLACEMENT? Placement { get; set; }

    public void Reload() {
      if (this.windowPlacementSetting != null) {
        this.Placement = this.windowPlacementSetting.Placement;
      }
    }

    public void Save() {
      if (this.windowPlacementSetting != null) {
        this.windowPlacementSetting.Placement = this.Placement;
      }
    }
  }
}