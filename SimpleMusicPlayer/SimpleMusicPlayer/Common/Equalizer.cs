using System.Collections.ObjectModel;
using System.Globalization;
using FMOD;
using SimpleMusicPlayer.Base;
using System.Linq;

namespace SimpleMusicPlayer.Common
{
  public class Equalizer : ViewModelBase
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
    private bool isEnabled = true;
    private SMPSettings smpSettings;

    private Equalizer(FMOD.System system, SMPSettings settings) {
      this.smpSettings = settings;
      this.Name = "DefaultEqualizer";
      this.fmodSystem = system;
      this.Bands = new ObservableCollection<EqualizerBand>();
      this.isEnabled = settings.PlayerSettings.EqualizerSettings == null || settings.PlayerSettings.EqualizerSettings.IsEnabled;
    }

    public static Equalizer GetEqualizer(FMOD.System system, SMPSettings settings) {
      var eq = new Equalizer(system, settings);
      eq.Init(system);
      return eq;
    }

    private void Init(FMOD.System system, bool setToDefaultValues = false) {
      var result = system.lockDSP();
      result.ERRCHECK();

      this.Bands.Clear();
      var gainValues = !setToDefaultValues && this.smpSettings.PlayerSettings.EqualizerSettings != null ? this.smpSettings.PlayerSettings.EqualizerSettings.GainValues : null;
      foreach (var value in EqDefaultValues) {
        var band = EqualizerBand.GetEqualizerBand(system, this.IsEnabled, value[0], value[1], value[2]);
        if (band != null) {
          float savedValue;
          if (gainValues != null && gainValues.TryGetValue(band.BandCaption, out savedValue)) {
            band.Gain = savedValue;
          }
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

      result = system.unlockDSP();
      result.ERRCHECK();
    }

    public void CleanUp() {
      this.DeInit(this.fmodSystem);
      this.Bands.Clear();
      this.fmodSystem = null;
      this.smpSettings = null;
    }

    public void SaveEqualizerSettings() {
      if (this.smpSettings != null) {
        var equalizerSettings = this.smpSettings.PlayerSettings.EqualizerSettings;
        if (equalizerSettings == null) {
          equalizerSettings = new EqualizerSettings {Name = this.Name};
          this.smpSettings.PlayerSettings.EqualizerSettings = equalizerSettings;
        }
        equalizerSettings.GainValues = this.Bands.ToDictionary(b => b.BandCaption, b => b.Gain);
        equalizerSettings.IsEnabled = this.IsEnabled;
      }
    }

    public ObservableCollection<EqualizerBand> Bands { get; private set; }

    public bool IsEnabled {
      get { return this.isEnabled; }
      set {
        if (Equals(value, this.isEnabled)) {
          return;
        }
        this.isEnabled = value;

        if (value) {
          this.Init(this.fmodSystem);
        } else {
          this.SaveEqualizerSettings();
          this.DeInit(this.fmodSystem);
        }

        this.OnPropertyChanged(() => this.IsEnabled);
      }
    }

    public string Name { get; private set; }

    public void SetToDefault() {
      this.DeInit(this.fmodSystem);
      this.Init(this.fmodSystem, true);
    }
  }

  public class EqualizerBand : ViewModelBase
  {
    private FMOD.DSP dspEQ;
    private float gain;
    private bool isActive;

    private EqualizerBand(DSP dspParamEq, float centerValue, float gainValue, bool active) {
      this.dspEQ = dspParamEq;
      if (centerValue >= 1000) {
        this.BandCaption = string.Format("{0}K", (centerValue / 1000));
      } else {
        this.BandCaption = centerValue.ToString(CultureInfo.InvariantCulture);
      }
      this.gain = gainValue;
      this.IsActive = active;
    }

    public static EqualizerBand GetEqualizerBand(FMOD.System system, bool isActive, float centerValue, float bandwithValue, float gainValue) {
      FMOD.DSPConnection dspConnTemp = null;
      FMOD.DSP dspParamEq = null;

      if (isActive) {
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
      }

      var band = new EqualizerBand(dspParamEq, centerValue, gainValue, isActive);
      return band;
    }

    public void Remove() {
      if (this.dspEQ != null) {
        var result = this.dspEQ.remove();
        result.ERRCHECK();
        this.dspEQ = null;
      }
      this.IsActive = false;
    }

    public string BandCaption { get; set; }

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
          var result = this.dspEQ.setActive(false);
          result.ERRCHECK();

          result = this.dspEQ.setParameter((int)FMOD.DSP_PARAMEQ.GAIN, value);
          result.ERRCHECK();

          result = this.dspEQ.setActive(true);
          result.ERRCHECK();
        }

        this.OnPropertyChanged(() => this.Gain);
      }
    }

    public bool IsActive {
      get { return this.isActive; }
      set {
        if (Equals(value, this.isActive)) {
          return;
        }
        this.isActive = value;
        this.OnPropertyChanged(() => this.IsActive);
      }
    }
  }
}