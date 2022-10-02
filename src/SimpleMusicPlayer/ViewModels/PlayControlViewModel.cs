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

        public PlayControlViewModel(MainViewModel mainViewModel)
        {
            var container = TinyIoCContainer.Current;
            this.playListsViewModel = mainViewModel.PlayListsViewModel;
            this.PlayerEngine = container.Resolve<PlayerEngine>();
            this.PlayerSettings = container.Resolve<PlayerSettings>();

            this.PlayerEngine.PlayNextFileAction = () =>
            {
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

            this.PlayOrPauseCommand = new DelegateCommand(this.PlayOrPause, this.CanPlayOrPause);
            this.StopCommand = new DelegateCommand(this.Stop, this.CanStop);
            this.PlayPrevCommand = new DelegateCommand(this.PlayPrev, this.CanPlayPrev);
            this.PlayNextCommand = new DelegateCommand(this.PlayNext, this.CanPlayNext);

            var playerInitialized = this.WhenAnyValue(x => x.PlayerEngine.Initializied);

            this.ShuffleCommand = ReactiveCommand.Create(
                () => this.PlayerSettings.PlayerEngine.ShuffleMode = !this.PlayerSettings.PlayerEngine.ShuffleMode,
                playerInitialized);

            this.RepeatCommand = ReactiveCommand.Create(
                () => this.PlayerSettings.PlayerEngine.RepeatMode = !this.PlayerSettings.PlayerEngine.RepeatMode,
                playerInitialized);

            this.MuteCommand = ReactiveCommand.Create(
                () => this.PlayerEngine.IsMute = !this.PlayerEngine.IsMute,
                playerInitialized);

            this.ShowMediaLibraryCommand = ReactiveCommand.Create(
                () => this.ShowMediaLibrary(),
                playerInitialized);

            this.ShowEqualizerCommand = ReactiveCommand.CreateFromTask(
                () => this.ShowEqualizer(),
                this.WhenAnyValue(x => x.IsEqualizerOpen, x => x.PlayerEngine.Initializied,
                    (isopen, initialized) => !isopen && initialized));
        }

        public PlayerEngine PlayerEngine { get; private set; }

        public PlayerSettings PlayerSettings { get; private set; }

        private MedialibView medialibView;

        private void ShowMediaLibrary()
        {
            if (this.medialibView != null)
            {
                this.medialibView.Activate();
            }
            else
            {
                this.medialibView = TinyIoCContainer.Current.Resolve<MedialibView>();
                this.medialibView.Closed += (sender, args) => this.medialibView = null;
                this.medialibView.Show();
            }
        }

        public ICommand PlayOrPauseCommand { get; }

        private bool CanPlayOrPause()
        {
            if (!this.PlayerEngine.Initializied)
            {
                return false;
            }

            var canPlay = (this.PlayerEngine.CurrentMediaFile != null && this.PlayerEngine.State != PlayerState.Play)
                          || (this.playListsViewModel.FirstSimplePlaylistFiles != null && this.playListsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any());
            var canPause = this.PlayerEngine.CurrentMediaFile != null && this.PlayerEngine.State == PlayerState.Play;
            return canPlay || canPause;
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

        public ICommand StopCommand { get; }

        private bool CanStop()
        {
            return this.PlayerEngine.Initializied && this.PlayerEngine.CurrentMediaFile != null;
        }

        private void Stop()
        {
            this.PlayerEngine.Stop();
            // at this we should re-set the current playlist item and the handsome selected file
            this.playListsViewModel.ResetCurrentItemAndSelection();
            CommandManager.InvalidateRequerySuggested();
        }

        public ICommand PlayPrevCommand { get; }

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

        public ICommand PlayNextCommand { get; }

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

        public ReactiveCommand<Unit, bool> ShuffleCommand { get; private set; }

        public ReactiveCommand<Unit, bool> RepeatCommand { get; private set; }

        public ReactiveCommand<Unit, bool> MuteCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> ShowMediaLibraryCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> ShowEqualizerCommand { get; private set; }

        private bool isEqualizerOpen;

        public bool IsEqualizerOpen
        {
            get => this.isEqualizerOpen;
            set => this.RaiseAndSetIfChanged(ref isEqualizerOpen, value);
        }

        private async Task ShowEqualizer()
        {
            this.IsEqualizerOpen = true;
            var view = new EqualizerView() { ViewModel = new EqualizerViewModel(this.PlayerEngine.Equalizer) };
            view.ClosingFinished += (sender, args) => this.IsEqualizerOpen = false;
            await ((MetroWindow)Application.Current.MainWindow).ShowChildWindowAsync(view);
        }

        public bool HandleKeyDown(KeyEventArgs args)
        {
            var handled = false;
            switch (args.Key)
            {
                case Key.R:
                    break;
                case Key.S:
                    break;
                case Key.Right:
                    this.PlayerEngine.SetCurrentPosition(this.PlayerEngine.CurrentPositionMs + 5000);
                    break;
                case Key.Left:
                    var newPos = (long)this.PlayerEngine.CurrentPositionMs - 5000;
                    this.PlayerEngine.SetCurrentPosition((uint)(newPos < 0 ? 0 : newPos));
                    break;
                case Key.Up:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        this.PlayerEngine.Volume = Math.Min(100, this.PlayerEngine.Volume + 5);
                        handled = true;
                    }

                    break;
                case Key.Down:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        this.PlayerEngine.Volume = Math.Max(0, this.PlayerEngine.Volume - 5);
                        handled = true;
                    }

                    break;
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        this.PlayerEngine.SetCurrentPosition(GetPositionByKey(args.Key, Key.D0));
                    }

                    break;
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    this.PlayerEngine.SetCurrentPosition(GetPositionByKey(args.Key, Key.NumPad0));

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
                    break;
                case Key.Space:
                    handled = this.PlayOrPauseCommand.CanExecute(null);
                    if (handled)
                    {
                        this.PlayOrPauseCommand.Execute(null);
                    }

                    break;
                case Key.E:
                    break;
            }

            return handled;
        }

        private uint GetPositionByKey(Key key, Key reference)
        {
            return (uint)(this.PlayerEngine.LengthMs * ((key - reference) / 10d));
        }
    }
}