using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ControlzEx.Standard;
using Newtonsoft.Json;
using ReactiveUI;
using SchwabenCode.QuickIO;
using SimpleMusicPlayer.Core.Interfaces;
using Splat;
using TinyIoC;

namespace SimpleMusicPlayer.Core.Player
{
    public static class PlayerSettingsExtensions
    {
        public static PlayerSettings Update(this PlayerSettings settings)
        {
            try
            {
                var fileName = Path.Combine(TinyIoCContainer.Current.Resolve<AppHelper>().ApplicationPath, PlayerSettings.SettingsFileName);
                if (settings == null || !QuickIOFile.Exists(fileName))
                {
                    return settings;
                }
                LogHost.Default.Info("loading player settings from {0}", fileName);
                var jsonString = QuickIOFile.ReadAllText(fileName);
                var fromThis = JsonConvert.DeserializeObject<PlayerSettings>(jsonString);

                settings.MainWindow = fromThis.MainWindow;
                settings.Medialib = fromThis.Medialib;
                settings.PlayerEngine = fromThis.PlayerEngine;
            }
            catch (Exception exception)
            {
                LogHost.Default.ErrorException("could not load player settings", exception);
            }
            return settings;
        }
    }

    public class PlayerSettings
    {
        [JsonIgnore]
        public const string SettingsFileName = "settings.json";

        public PlayerSettings()
        {
            this.MainWindow = new MainWindowSettings();
            this.Medialib = new MedialibSettings();
            this.PlayerEngine = new PlayerEngineSettings();
        }

        public void Save()
        {
            try
            {
                var fileName = Path.Combine(TinyIoCContainer.Current.Resolve<AppHelper>().ApplicationPath, SettingsFileName);
                LogHost.Default.Info("saving player settings to {0}", fileName);
                var settingsAsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
                QuickIOFile.WriteAllText(fileName, settingsAsJson);
            }
            catch (Exception exception)
            {
                LogHost.Default.Error("could not save player settings", exception);
            }
        }

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

    public class MainWindowSettings : IWindowSetting
    {
        public WINDOWPLACEMENT Placement { get; set; }
        public DpiScale? DpiScale { get; set; }
    }

    public class MedialibSettings : IWindowSetting
    {
        public WINDOWPLACEMENT Placement { get; set; }
        public DpiScale? DpiScale { get; set; }
    }

    [JsonObject]
    public class PlayerEngineSettings : ReactiveObject
    {
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

        private bool shuffleMode;

        public bool ShuffleMode
        {
            get => this.shuffleMode;
            set => this.RaiseAndSetIfChanged(ref shuffleMode, value);
        }

        private bool repeatMode;

        public bool RepeatMode
        {
            get => this.repeatMode;
            set => this.RaiseAndSetIfChanged(ref repeatMode, value);
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