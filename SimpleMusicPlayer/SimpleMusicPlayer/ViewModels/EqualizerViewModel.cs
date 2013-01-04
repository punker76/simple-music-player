using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class EqualizerViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private Equalizer equalizer;

    public EqualizerViewModel(Equalizer equalizer) {
      this.Equalizer = equalizer;
    }

    public Equalizer Equalizer {
      get { return this.equalizer; }
      set {
        if (Equals(value, this.equalizer)) {
          return;
        }
        this.equalizer = value;
        this.OnPropertyChanged(() => this.Equalizer);
      }
    }
  }
}