using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.Core.Player;
using SimpleMusicPlayer.Views;
using TinyIoC;

namespace SimpleMusicPlayer.ViewModels
{
    public class PlayControlViewModel : ReactiveObject, IKeyHandler
    {
        private readonly PlayListsViewModel playListsViewModel;
        private ICommand playOrPauseCommand;
        private ICommand stopCommand;
        private ICommand playPrevCommand;
        private ICommand playNextCommand;

        public PlayControlViewModel(MainViewModel mainViewModel)
        {
            var container = TinyIoCContainer.Current;
            this.playListsViewModel = mainViewModel.PlayListsViewModel;
            this.PlayerEngine = container.Resolve<PlayerEngine>();
            this.PlayerSettings = container.Resolve<PlayerSettings>();

            this.PlayerEngine.PlayNextFileAction = () => {
                var playerMustBeStoped = !this.CanPlayNext();
                if (!playerMustBeStoped)
                {
                    playerMustBeStoped = !this.PlayerSettings.PlayerEngine.ShuffleMode
                                         && !this.PlayerSettings.PlayerEngine.RepeatMode
                                         && this.playListsViewModel.IsLastPlayListFile();
                    if (!playerMustBeStoped)
                    {
                        this.PlayNext();
                    }
                }
                if (playerMustBeStoped)
                {
                    this.Stop();
                }
            };

            var playerInitialized = this.WhenAnyValue(x => x.PlayerEngine.Initializied);

            this.ShuffleCommand = ReactiveCommand.Create(playerInitialized);
            this.ShuffleCommand.Subscribe(x => {
                this.PlayerSettings.PlayerEngine.ShuffleMode = !this.PlayerSettings.PlayerEngine.ShuffleMode;
            });

            this.RepeatCommand = ReactiveCommand.Create(playerInitialized);
            this.RepeatCommand.Subscribe(x => {
                this.PlayerSettings.PlayerEngine.RepeatMode = !this.PlayerSettings.PlayerEngine.RepeatMode;
            });

            this.MuteCommand = ReactiveCommand.Create(playerInitialized);
            this.MuteCommand.Subscribe(x => {
                this.PlayerEngine.IsMute = !this.PlayerEngine.IsMute;
            });

            this.ShowMediaLibraryCommand = ReactiveCommand.Create(playerInitialized);

            this.ShowEqualizerCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.IsEqualizerOpen, x => x.PlayerEngine.Initializied,
                                                                                          (isopen, initialized) => !isopen && initialized),
                                                                        x => ShowEqualizer());
        }

        public PlayerEngine PlayerEngine { get; private set; }

        public PlayerSettings PlayerSettings { get; private set; }

        public ICommand PlayOrPauseCommand
        {
            get { return this.playOrPauseCommand ?? (this.playOrPauseCommand = new DelegateCommand(this.PlayOrPause, this.CanPlayOrPause)); }
        }

        private bool CanPlayOrPause()
        {
            return this.PlayerEngine.Initializied
                   && this.playListsViewModel.FirstSimplePlaylistFiles != null
                   && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
        }

        private void PlayOrPause()
        {
            if (this.PlayerEngine.State == PlayerState.Pause || this.PlayerEngine.State == PlayerState.Play)
            {
                this.PlayerEngine.Pause();
            }
            else
            {
                var file = this.playListsViewModel.GetCurrentPlayListFile();
                if (file != null)
                {
                    this.PlayerEngine.Play(file);
                }
            }
        }

        public ICommand StopCommand
        {
            get { return this.stopCommand ?? (this.stopCommand = new DelegateCommand(this.Stop, this.CanStop)); }
        }

        private bool CanStop()
        {
            return this.PlayerEngine.Initializied
                   && this.playListsViewModel.FirstSimplePlaylistFiles != null
                   && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
        }

        private void Stop()
        {
            this.PlayerEngine.Stop();
            // at this we should re-set the current playlist item and the handsome selected file
            this.playListsViewModel.ResetCurrentItemAndSelection();
        }

        public ICommand PlayPrevCommand
        {
            get { return this.playPrevCommand ?? (this.playPrevCommand = new DelegateCommand(this.PlayPrev, this.CanPlayPrev)); }
        }

        private bool CanPlayPrev()
        {
            return this.PlayerEngine.Initializied
                   && this.playListsViewModel.FirstSimplePlaylistFiles != null
                   && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
        }

        private void PlayPrev()
        {
            var file = this.playListsViewModel.GetPrevPlayListFile();
            if (file != null)
            {
                this.PlayerEngine.Play(file);
            }
        }

        public ICommand PlayNextCommand
        {
            get { return this.playNextCommand ?? (this.playNextCommand = new DelegateCommand(this.PlayNext, this.CanPlayNext)); }
        }

        private bool CanPlayNext()
        {
            return this.PlayerEngine.Initializied
                   && this.playListsViewModel.FirstSimplePlaylistFiles != null
                   && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
        }

        private void PlayNext()
        {
            var file = this.playListsViewModel.GetNextPlayListFile();
            if (file != null)
            {
                this.PlayerEngine.Play(file);
            }
        }

        public ReactiveCommand<object> ShuffleCommand { get; private set; }

        public ReactiveCommand<object> RepeatCommand { get; private set; }

        public ReactiveCommand<object> MuteCommand { get; private set; }

        public ReactiveCommand<object> ShowMediaLibraryCommand { get; private set; }

        public ReactiveCommand<Unit> ShowEqualizerCommand { get; private set; }

        private bool isEqualizerOpen;

        public bool IsEqualizerOpen
        {
            get { return this.isEqualizerOpen; }
            set { this.RaiseAndSetIfChanged(ref isEqualizerOpen, value); }
        }

        private async Task ShowEqualizer()
        {
            this.IsEqualizerOpen = true;
            var view = new EqualizerView() { ViewModel = new EqualizerViewModel(this.PlayerEngine.Equalizer) };
            view.ClosingFinished += (sender, args) => this.IsEqualizerOpen = false;
            await ((MetroWindow)Application.Current.MainWindow).ShowChildWindowAsync(view);
        }

        public bool HandleKeyDown(Key key)
        {
            var handled = false;
            switch (key)
            {
                case Key.R:
                    handled = this.RepeatCommand.CanExecute(null);
                    if (handled)
                    {
                        this.RepeatCommand.Execute(null);
                    }
                    break;
                case Key.S:
                    handled = this.ShuffleCommand.CanExecute(null);
                    if (handled)
                    {
                        this.ShuffleCommand.Execute(null);
                    }
                    break;
                case Key.J:
                    handled = this.PlayNextCommand.CanExecute(null);
                    if (handled)
                    {
                        this.PlayNextCommand.Execute(null);
                    }
                    break;
                case Key.K:
                    handled = this.PlayPrevCommand.CanExecute(null);
                    if (handled)
                    {
                        this.PlayPrevCommand.Execute(null);
                    }
                    break;
                case Key.M:
                    handled = this.MuteCommand.CanExecute(null);
                    if (handled)
                    {
                        this.MuteCommand.Execute(null);
                    }
                    break;
                case Key.Space:
                    handled = this.PlayOrPauseCommand.CanExecute(null);
                    if (handled)
                    {
                        this.PlayOrPauseCommand.Execute(null);
                    }
                    break;
                case Key.L:
                    handled = this.ShowMediaLibraryCommand.CanExecute(null);
                    if (handled)
                    {
                        this.ShowMediaLibraryCommand.Execute(null);
                    }
                    break;
                case Key.E:
                    handled = this.ShowEqualizerCommand.CanExecute(null);
                    if (handled)
                    {
                        this.ShowEqualizerCommand.Execute(null);
                    }
                    break;
            }
            return handled;
        }
    }
}