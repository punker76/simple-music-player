using System;
using System.Globalization;
using FMOD;
using ReactiveUI;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
    public class EqualizerBand : ReactiveObject
    {
        private FMOD.System fmodSystem;
        private FMOD.DSP dspEQ;

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

            this.WhenAnyValue(x => x.Gain)
                .Subscribe(newGain => {
                    if (this.IsActive && this.dspEQ.hasHandle())
                    {
                        System.Diagnostics.Debug.WriteLine(">> Gain value: " + newGain);

                        this.dspEQ.setActive(false).ERRCHECK();
                        this.dspEQ.setParameterFloat((int)FMOD.DSP_PARAMEQ.GAIN, newGain).ERRCHECK();
                        this.dspEQ.setActive(true).ERRCHECK();
                        this.fmodSystem.update().ERRCHECK();
                    }
                });

            this.Gain = gainValue;
            this.IsActive = active;
        }

        public static EqualizerBand GetEqualizerBand(FMOD.System system, bool isActive, float centerValue, float bandwithValue, float gainValue)
        {
            FMOD.DSP dspParamEq = default;

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

        public void Release()
        {
            if (this.dspEQ.hasHandle())
            {
                this.dspEQ.setActive(false).ERRCHECK();

                FMOD.ChannelGroup masterChannelGroup;
                this.fmodSystem.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();

                masterChannelGroup.removeDSP(this.dspEQ).ERRCHECK();

                this.dspEQ.release().ERRCHECK();
                this.dspEQ.clearHandle();
            }
            this.IsActive = false;
        }

        public string BandCaption { get; set; }

        private float gain;
        /// <summary>
        /// Gain: (Type:float) - Frequency Gain in dB. -30 to 30. Default = 0.
        /// </summary>
        public float Gain
        {
            get => this.gain;
            set => this.RaiseAndSetIfChanged(ref gain, value);
        }

        private bool isActive;

        public bool IsActive
        {
            get => this.isActive;
            set => this.RaiseAndSetIfChanged(ref isActive, value);
        }
    }
}