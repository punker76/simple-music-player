using System.Reactive;
using ReactiveUI;
using SimpleMusicPlayer.Core.Player;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace SimpleMusicPlayer.ViewModels
{
    public class EqualizerViewModel : ReactiveObject
    {
        public EqualizerViewModel(Equalizer equalizer)
        {
            this.Equalizer = equalizer;

            this.SetToDefaultCommand = ReactiveCommand.Create(
                () => this.Equalizer.SetToDefault(),
                this.WhenAnyValue(x => x.Equalizer.IsEnabled));

            this.CloseEqualizerCommand = ReactiveCommand.Create(
                () => this.Equalizer.SaveEqualizerSettings());
        }

        public Equalizer Equalizer { get; private set; }

        public ReactiveCommand<Unit, Unit> SetToDefaultCommand { get; protected set; }

        public ReactiveCommand<Unit, Unit> CloseEqualizerCommand { get; protected set; }
    }
}