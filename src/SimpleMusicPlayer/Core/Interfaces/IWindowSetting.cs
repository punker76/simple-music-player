using System.Windows;
using ControlzEx.Standard;

namespace SimpleMusicPlayer.Core.Interfaces
{
    public interface IWindowSetting
    {
        WINDOWPLACEMENT Placement { get; set; }
        DpiScale? DpiScale { get; set; }
    }
}