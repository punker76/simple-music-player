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
                    break;
                case Key.S:
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
    }
}