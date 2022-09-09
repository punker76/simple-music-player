using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
    public class Equalizer : ReactiveObject
    {
        /// <summary>
        /// FMOD_DSP_PARAMEQ_CENTER -> (Type:float) - Frequency center. 20.0 to 22000.0. Default = 8000.0. 
        /// FMOD_DSP_PARAMEQ_BANDWIDTH -> (Type:float) - Octave range around the center frequency to filter. 0.2 to 5.0. Default = 1.0. 
        /// FMOD_DSP_PARAMEQ_GAIN -> (Type:float) - Frequency Gain in dB. -30 to 30. Default = 0. 
        /// </summary>
        private static readonly float[][] EqDefaultValues = new[]
                                                            {
                                                                new[] { 32f, 1f, 0f },
                                                                new[] { 64f, 1f, 0f },
                                                                new[] { 125f, 1f, 0f },
                                                                new[] { 250f, 1f, 0f },
                                                                new[] { 500f, 1f, 0f },
                                                                new[] { 1000f, 1f, 0f },
                                                                new[] { 2000f, 1f, 0f },
                                                                new[] { 4000f, 1f, 0f },
                                                                new[] { 8000f, 1f, 0f },
                                                                new[] { 16000f, 1f, 0f }
                                                            };

        private FMOD.System fmodSystem;
        private PlayerSettings playerSettings;

        private Equalizer(FMOD.System system, PlayerSettings settings)
        {
            this.playerSettings = settings;
            this.Name = "DefaultEqualizer";
            this.fmodSystem = system;
            this.Initializied = false;
            this.Bands = new ObservableCollection<EqualizerBand>();
            this.IsEnabled = settings.PlayerEngine.EqualizerSettings == null || settings.PlayerEngine.EqualizerSettings.IsEnabled;

            this.WhenAnyValue(x => x.IsEnabled)
                .Subscribe(enabled => {
                    if (!this.Initializied || enabled)
                    {
                        this.Init(this.fmodSystem);
                    }
                    else if (this.Initializied)
                    {
                        this.SaveEqualizerSettings();
                        this.DeInit(this.fmodSystem);
                    }
                });
        }

        public static Equalizer GetEqualizer(FMOD.System system, PlayerSettings settings)
        {
            var eq = new Equalizer(system, settings);
            eq.Initializied = true;
            return eq;
        }

        private void Init(FMOD.System system, bool setToDefaultValues = false)
        {
            system.lockDSP().ERRCHECK();

            this.Bands.Clear();
            var gainValues = !setToDefaultValues && this.playerSettings.PlayerEngine.EqualizerSettings != null
                ? this.playerSettings.PlayerEngine.EqualizerSettings.GainValues
                : null;
            foreach (var value in EqDefaultValues)
            {
                var band = EqualizerBand.GetEqualizerBand(system, this.IsEnabled, value[0], value[1], value[2]);
                if (band != null)
                {
                    float savedValue;
                    if (gainValues != null && gainValues.TryGetValue(band.BandCaption, out savedValue))
                    {
                        band.Gain = savedValue;
                    }
                    this.Bands.Add(band);
                }
            }

            system.unlockDSP().ERRCHECK();
            system.update().ERRCHECK();
        }

        private void DeInit(FMOD.System system)
        {
            system.lockDSP().ERRCHECK();

            foreach (var band in this.Bands)
            {
                band.Release();
            }

            system.unlockDSP().ERRCHECK();
            system.update().ERRCHECK();
        }

        public void CleanUp()
        {
            this.DeInit(this.fmodSystem);
            this.Bands.Clear();
            this.playerSettings = null;
        }

        public void SaveEqualizerSettings()
        {
            if (this.playerSettings != null)
            {
                var equalizerSettings = this.playerSettings.PlayerEngine.EqualizerSettings;
                if (equalizerSettings == null)
                {
                    equalizerSettings = new EqualizerSettings { Name = this.Name };
                    this.playerSettings.PlayerEngine.EqualizerSettings = equalizerSettings;
                }
                equalizerSettings.GainValues = this.Bands.ToDictionary(b => b.BandCaption, b => b.Gain);
                equalizerSettings.IsEnabled = this.IsEnabled;
            }
        }

        public ObservableCollection<EqualizerBand> Bands { get; private set; }

        private bool initializied;

        public bool Initializied
        {
            get => this.initializied;
            set => this.RaiseAndSetIfChanged(ref initializied, value);
        }

        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }

        public string Name { get; private set; }

        public void SetToDefault()
        {
            this.DeInit(this.fmodSystem);
            this.Init(this.fmodSystem, true);
        }
    }
}