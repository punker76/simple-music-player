using System.Collections.ObjectModel;
using System.Globalization;
using FMOD;
using SimpleMusicPlayer.Base;
using System.Linq;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Common
{
    public class Equalizer : ViewModelBase
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
        private bool isEnabled = true;
        private PlayerSettings playerSettings;

        private Equalizer(FMOD.System system, PlayerSettings settings)
        {
            this.playerSettings = settings;
            this.Name = "DefaultEqualizer";
            this.fmodSystem = system;
            this.Bands = new ObservableCollection<EqualizerBand>();
            this.isEnabled = settings.PlayerEngine.EqualizerSettings == null || settings.PlayerEngine.EqualizerSettings.IsEnabled;
        }

        public static Equalizer GetEqualizer(FMOD.System system, PlayerSettings settings)
        {
            var eq = new Equalizer(system, settings);
            eq.Init(system);
            return eq;
        }

        private void Init(FMOD.System system, bool setToDefaultValues = false)
        {
            system.lockDSP().ERRCHECK();

            this.Bands.Clear();
            var gainValues = !setToDefaultValues && this.playerSettings.PlayerEngine.EqualizerSettings != null ? this.playerSettings.PlayerEngine.EqualizerSettings.GainValues : null;
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
                band.Remove();
            }

            system.unlockDSP().ERRCHECK();
            system.update().ERRCHECK();
        }

        public void CleanUp()
        {
            this.DeInit(this.fmodSystem);
            this.Bands.Clear();
            this.fmodSystem = null;
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

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                if (Equals(value, this.isEnabled))
                {
                    return;
                }
                this.isEnabled = value;

                if (value)
                {
                    this.Init(this.fmodSystem);
                }
                else
                {
                    this.SaveEqualizerSettings();
                    this.DeInit(this.fmodSystem);
                }

                this.OnPropertyChanged(() => this.IsEnabled);
            }
        }

        public string Name { get; private set; }

        public void SetToDefault()
        {
            this.DeInit(this.fmodSystem);
            this.Init(this.fmodSystem, true);
        }
    }

    public class EqualizerBand : ViewModelBase
    {
        private FMOD.System fmodSystem;
        private FMOD.DSP dspEQ;
        private float gain;
        private bool isActive;

        private EqualizerBand(FMOD.System system, DSP dspParamEq, float centerValue, float gainValue, bool active)
        {
            this.fmodSystem = system;

            this.dspEQ = dspParamEq;
            if (centerValue >= 1000)
            {
                this.BandCaption = string.Format("{0}K", (centerValue / 1000));
            }
            else
            {
                this.BandCaption = centerValue.ToString(CultureInfo.InvariantCulture);
            }
            this.gain = gainValue;
            this.IsActive = active;
        }

        public static EqualizerBand GetEqualizerBand(FMOD.System system, bool isActive, float centerValue, float bandwithValue, float gainValue)
        {
            FMOD.DSP dspParamEq = null;

            if (isActive)
            {
                if (!system.createDSPByType(FMOD.DSP_TYPE.PARAMEQ, out dspParamEq).ERRCHECK())
                {
                    return null;
                }

                FMOD.ChannelGroup masterChannelGroup;
                if (!system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK())
                {
                    return null;
                }

                int numDSPs;
                if (!masterChannelGroup.getNumDSPs(out numDSPs).ERRCHECK())
                {
                    return null;
                }

                if (!masterChannelGroup.addDSP(numDSPs, dspParamEq).ERRCHECK())
                {
                    return null;
                }

                if (!dspParamEq.setParameterFloat((int)FMOD.DSP_PARAMEQ.CENTER, centerValue).ERRCHECK())
                {
                    return null;
                }

                if (!dspParamEq.setParameterFloat((int)FMOD.DSP_PARAMEQ.BANDWIDTH, bandwithValue).ERRCHECK())
                {
                    return null;
                }

                if (!dspParamEq.setParameterFloat((int)FMOD.DSP_PARAMEQ.GAIN, gainValue).ERRCHECK())
                {
                    return null;
                }

                if (!dspParamEq.setActive(true).ERRCHECK())
                {
                    return null;
                }
            }

            var band = new EqualizerBand(system, dspParamEq, centerValue, gainValue, isActive);
            return band;
        }

        public void Remove()
        {
            if (this.dspEQ != null)
            {
                this.dspEQ.setActive(false).ERRCHECK();

                FMOD.ChannelGroup masterChannelGroup = null;
                this.fmodSystem.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();

                masterChannelGroup.removeDSP(this.dspEQ).ERRCHECK();

                this.dspEQ.release().ERRCHECK();

                this.dspEQ = null;
                this.fmodSystem = null;
            }
            this.IsActive = false;
        }

        public string BandCaption { get; set; }

        /// <summary>
        /// Gain: Frequency Gain. 0.05 to 3.0. Default = 1.0
        /// </summary>
        public float Gain
        {
            get { return this.gain; }
            set
            {
                if (Equals(value, this.gain))
                {
                    return;
                }
                this.gain = value;
                System.Diagnostics.Debug.WriteLine(">> Gain value: " + value);

                if (this.dspEQ != null)
                {
                    this.dspEQ.setActive(false).ERRCHECK();

                    this.dspEQ.setParameterFloat((int)FMOD.DSP_PARAMEQ.GAIN, value).ERRCHECK();

                    this.dspEQ.setActive(true).ERRCHECK();

                    this.fmodSystem.update().ERRCHECK();
                }

                this.OnPropertyChanged(() => this.Gain);
            }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set
            {
                if (Equals(value, this.isActive))
                {
                    return;
                }
                this.isActive = value;
                this.OnPropertyChanged(() => this.IsActive);
            }
        }
    }
}