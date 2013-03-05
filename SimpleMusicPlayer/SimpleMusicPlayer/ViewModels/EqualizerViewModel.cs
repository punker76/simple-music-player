using System.Windows.Input;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class EqualizerViewModel : ViewModelBase
  {
    private Equalizer equalizer;
    private ICommand setToDefaultCommand;

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

    public ICommand SetToDefaultCommand {
      get { return this.setToDefaultCommand ?? (this.setToDefaultCommand = new DelegateCommand(this.SetToDefault, this.CanSetToDefault)); }
    }

    private bool CanSetToDefault() {
      return this.Equalizer.IsEnabled;
    }

    private void SetToDefault() {
      this.Equalizer.SetToDefault();
    }
  }
}