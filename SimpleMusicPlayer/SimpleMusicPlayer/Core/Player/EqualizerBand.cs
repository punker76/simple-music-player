using System.Globalization;
using FMOD;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
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