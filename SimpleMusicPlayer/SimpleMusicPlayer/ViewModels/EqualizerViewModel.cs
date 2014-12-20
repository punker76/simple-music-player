using System.Windows.Input;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Player;

namespace SimpleMusicPlayer.ViewModels
{
    public class EqualizerViewModel : ViewModelBase
    {
        private Equalizer equalizer;
        private ICommand setToDefaultCommand;
        private ICommand closeEqualizerCommand;

        public EqualizerViewModel(Equalizer equalizer)
        {
            this.Equalizer = equalizer;
        }

        public Equalizer Equalizer
        {
            get { return this.equalizer; }
            set
            {
                if (Equals(value, this.equalizer))
                {
                    return;
                }
                this.equalizer = value;
                this.OnPropertyChanged(() => this.Equalizer);
            }
        }

        public ICommand SetToDefaultCommand
        {
            get { return this.setToDefaultCommand ?? (this.setToDefaultCommand = new DelegateCommand(this.SetToDefault, this.CanSetToDefault)); }
        }

        private bool CanSetToDefault()
        {
            return this.Equalizer.IsEnabled;
        }

        private void SetToDefault()
        {
            this.Equalizer.SetToDefault();
        }

        public ICommand CloseEqualizerCommand
        {
            get { return this.closeEqualizerCommand ?? (this.closeEqualizerCommand = new DelegateCommand(this.CloseEqualizer, this.CanCloseEqualizer)); }
        }

        private bool CanCloseEqualizer()
        {
            return true;
        }

        private void CloseEqualizer()
        {
            if (this.Equalizer.IsEnabled)
            {
                this.Equalizer.SaveEqualizerSettings();
            }
        }
    }
}