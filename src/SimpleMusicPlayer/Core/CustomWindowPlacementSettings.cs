using MahApps.Metro.Controls;
using MahApps.Metro.Native;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core
{
    public class CustomWindowPlacementSettings : IWindowPlacementSettings
    {
        private readonly IWindowPlacementSetting windowPlacementSetting;

        public CustomWindowPlacementSettings(IWindowPlacementSetting wps)
        {
            this.windowPlacementSetting = wps;
        }

        public WINDOWPLACEMENT? Placement { get; set; }

        public bool UpgradeSettings { get; set; }

        public void Reload()
        {
            if (this.windowPlacementSetting != null)
            {
                this.Placement = this.windowPlacementSetting.Placement;
            }
        }

        public void Upgrade()
        {
        }

        public void Save()
        {
            if (this.windowPlacementSetting != null)
            {
                this.windowPlacementSetting.Placement = this.Placement;
            }
        }
    }
}