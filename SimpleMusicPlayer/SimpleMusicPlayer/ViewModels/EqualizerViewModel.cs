using System;
using ReactiveUI;
using SimpleMusicPlayer.Core.Player;

namespace SimpleMusicPlayer.ViewModels
{
    public class EqualizerViewModel : ReactiveObject
    {
        public EqualizerViewModel(Equalizer equalizer)
        {
            this.Equalizer = equalizer;

            SetToDefaultCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Equalizer.IsEnabled));
            SetToDefaultCommand.Subscribe(_ => Equalizer.SetToDefault());

            CloseEqualizerCommand = ReactiveCommand.Create();
            CloseEqualizerCommand.Subscribe(_ => {
                if (this.Equalizer.IsEnabled)
                {
                    this.Equalizer.SaveEqualizerSettings();
                }
            });
        }

        public Equalizer Equalizer { get; private set; }

        public ReactiveCommand<object> SetToDefaultCommand { get; protected set; }

        public ReactiveCommand<object> CloseEqualizerCommand { get; protected set; }
    }
}