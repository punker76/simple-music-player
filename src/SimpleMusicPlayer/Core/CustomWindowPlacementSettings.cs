using System.Windows;
using System.Windows.Media;
using ControlzEx.Standard;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Core.Interfaces;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public class CustomWindowPlacementSettings : IWindowPlacementSettings, IEnableLogger
    {
        private readonly IWindowSetting _windowSetting;

        public CustomWindowPlacementSettings(IWindowSetting wps)
        {
            this._windowSetting = wps;
        }

        public WINDOWPLACEMENT Placement { get; set; }

        public bool UpgradeSettings { get; set; }

        public void Reload()
        {
            if (this._windowSetting != null && this._windowSetting.Placement != null)
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    // this fixes wrong Dpi usage for SetWindowPlacement
                    mainWindow.Left = this._windowSetting.Placement.normalPosition.Left;
                    mainWindow.Top = this._windowSetting.Placement.normalPosition.Top;
                }
                this.Placement = this._windowSetting.Placement;
                this.Log().Debug("Loaded WINDOWPLACEMENT: width={0}, height={1}, dpi={2}", this.Placement.normalPosition.Width, this.Placement.normalPosition.Height, this._windowSetting.DpiScale?.PixelsPerDip);
            }
        }

        public void Upgrade()
        {
        }

        public void Save()
        {
            if (this._windowSetting != null && this.Placement != null)
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    this._windowSetting.DpiScale = VisualTreeHelper.GetDpi(mainWindow);
                }
                this.Log().Debug("Saved WINDOWPLACEMENT: width={0}, height={1}, dpi={2}", this.Placement.normalPosition.Width, this.Placement.normalPosition.Height, this._windowSetting.DpiScale?.PixelsPerDip);
                this._windowSetting.Placement = this.Placement;
            }
        }
    }
}