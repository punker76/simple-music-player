using System.Collections.Generic;
using MahApps.Metro.Native;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core.Player
{
    public class PlayerSettings
    {
        public MainWindowSettings MainWindow { get; set; }
        public MedialibSettings Medialib { get; set; }
        public PlayerEngineSettings PlayerEngine { get; set; }

        public static PlayerSettings GetEmptySettings()
        {
            return new PlayerSettings {
                MainWindow = new MainWindowSettings(),
                Medialib = new MedialibSettings(),
                PlayerEngine = new PlayerEngineSettings()
            };
        }
    }

    public class MainWindowSettings : IWindowPlacementSetting
    {
        public WINDOWPLACEMENT? Placement { get; set; }
    }

    public class MedialibSettings : IWindowPlacementSetting
    {
        public WINDOWPLACEMENT? Placement { get; set; }
    }

    public class PlayerEngineSettings : ViewModelBase
    {
        private bool shuffleMode;
        private bool repeatMode;

        public PlayerEngineSettings()
        {
            Volume = 100f;
            FadeIn = 5000f;
            FadeOut = 5000f;
            EqualizerSettings = new EqualizerSettings() { GainValues = new Dictionary<string, float>() };
        }

        public float Volume { get; set; }

        public float FadeIn { get; set; }
        public float FadeOut { get; set; }

        public bool Mute { get; set; }

        public bool ShuffleMode
        {
            get { return this.shuffleMode; }
            set
            {
                if (Equals(value, this.shuffleMode))
                {
                    return;
                }
                this.shuffleMode = value;
                this.OnPropertyChanged(() => this.ShuffleMode);
            }
        }

        public bool RepeatMode
        {
            get { return this.repeatMode; }
            set
            {
                if (Equals(value, this.repeatMode))
                {
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