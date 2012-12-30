using System.Collections.ObjectModel;
using FMOD;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer.Common
{
  public class Equalizer
  {
    // Center: Frequency center. 20.0 to 22000.0. Default = 8000.0
    // Bandwith: Octave range around the center frequency to filter. 0.2 to 5.0. Default = 1.0
    // Gain: Frequency Gain. 0.05 to 3.0. Default = 1.0
    private static readonly float[][] EqDefaultValues = new[] {
      new[] {32f, 1f, 1f},
      new[] {64f, 1f, 1f},
      new[] {125f, 1f, 1f},
      new[] {250f, 1f, 1f},
      new[] {500f, 1f, 1f},
      new[] {1000f, 1f, 1f},
      new[] {2000f, 1f, 1f},
      new[] {4000f, 1f, 1f},
      new[] {8000f, 1f, 1f},
      new[] {16000f, 1f, 1f}
    };

    private FMOD.System fmodSystem;

    private Equalizer(FMOD.System system) {
      this.fmodSystem = system;
      this.Bands = new ObservableCollection<EqualizerBand>();
    }

    public static Equalizer GetEqualizer(FMOD.System system) {
      var eq = new Equalizer(system);

      eq.Init(system);

      return eq;
    }

    private void Init(FMOD.System system) {
      var result = system.lockDSP();
      result.ERRCHECK();

      foreach (var value in EqDefaultValues) {
        var band = EqualizerBand.GetEqualizerBand(system, value[0], value[1], value[2]);
        if (band != null) {
          this.Bands.Add(band);
        }
      }

      result = system.unlockDSP();
      result.ERRCHECK();
    }

    private void DeInit(FMOD.System system) {
      var result = system.lockDSP();
      result.ERRCHECK();

      foreach (var band in this.Bands) {
        band.Remove();
      }
      this.Bands.Clear();

      result = system.unlockDSP();
      result.ERRCHECK();
    }

    public void CleanUp() {
      this.DeInit(this.fmodSystem);
      this.fmodSystem = null;
    }

    public ObservableCollection<EqualizerBand> Bands { get; private set; }
  }

  public class EqualizerBand : ViewModelBaseNotifyPropertyChanged
  {
    private FMOD.System fmodSystem;
    private FMOD.DSP dspEQ;
    private float gain;

    private EqualizerBand(FMOD.System system, DSP dspParamEq, float gainValue) {
      this.fmodSystem = system;
      this.dspEQ = dspParamEq;
      this.gain = gainValue;
    }

    public static EqualizerBand GetEqualizerBand(FMOD.System system, float centerValue, float bandwithValue, float gainValue) {
      FMOD.DSPConnection dspConnTemp = null;

      FMOD.DSP dspParamEq = null;
      var result = system.createDSPByType(FMOD.DSP_TYPE.PARAMEQ, ref dspParamEq);
      if (!result.ERRCHECK()) {
        return null;
      }

      result = system.addDSP(dspParamEq, ref dspConnTemp);
      if (!result.ERRCHECK()) {
        return null;
      }

      result = dspParamEq.setParameter((int)FMOD.DSP_PARAMEQ.CENTER, centerValue);
      if (!result.ERRCHECK()) {
        return null;
      }

      result = dspParamEq.setParameter((int)FMOD.DSP_PARAMEQ.BANDWIDTH, bandwithValue);
      if (!result.ERRCHECK()) {
        return null;
      }

      result = dspParamEq.setParameter((int)FMOD.DSP_PARAMEQ.GAIN, gainValue);
      if (!result.ERRCHECK()) {
        return null;
      }

      result = dspParamEq.setActive(true);
      if (!result.ERRCHECK()) {
        return null;
      }

      var band = new EqualizerBand(system, dspParamEq, gainValue);
      return band;
    }

    public void Remove() {
      var result = this.dspEQ.remove();
      result.ERRCHECK();
      this.dspEQ = null;
    }

    /// <summary>
    /// Gain: Frequency Gain. 0.05 to 3.0. Default = 1.0
    /// </summary>
    public float Gain {
      get { return this.gain; }
      set {
        if (Equals(value, this.gain)) {
          return;
        }
        this.gain = value;

        if (this.dspEQ != null) {
          //          result = dspParamEq.setActive(false);
          //          result.ERRCHECK();

          var result = this.dspEQ.setParameter((int)FMOD.DSP_PARAMEQ.GAIN, value);
          result.ERRCHECK();

          //          result = dspParamEq.setActive(true);
          //          result.ERRCHECK();
        }

        this.OnPropertyChanged(() => this.Gain);
      }
    }
  }
}