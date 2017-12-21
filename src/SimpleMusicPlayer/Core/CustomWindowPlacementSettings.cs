using ControlzEx.Standard;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Core.Interfaces;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public class CustomWindowPlacementSettings : IWindowPlacementSettings, IEnableLogger
    {
        private readonly IWindowPlacementSetting windowPlacementSetting;

        public CustomWindowPlacementSettings(IWindowPlacementSetting wps)
        {
            this.windowPlacementSetting = wps;
        }

        public WINDOWPLACEMENT Placement { get; set; }

        public bool UpgradeSettings { get; set; }

        public void Reload()
        {
            if (this.windowPlacementSetting != null)
            {
                this.Placement = this.windowPlacementSetting.Placement;
                this.Log().Info("Loaded WINDOWPLACEMENT = {0}", this.Placement?.normalPosition);
            }
        }

        public void Upgrade()
        {
        }

        public void Save()
        {
            if (this.windowPlacementSetting != null)
            {
                this.Log().Info("Saved WINDOWPLACEMENT = {0}", this.Placement?.normalPosition);
                this.windowPlacementSetting.Placement = this.Placement;
            }
        }
    }
}