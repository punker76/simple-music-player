using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.Core.Player;
using SimpleMusicPlayer.Views;

namespace SimpleMusicPlayer.ViewModels
{
    public class MainViewModel : ReactiveObject, IKeyHandler
    {
        private ICommand showOnGitHubCmd;
        private MedialibView medialibView;

        public MainViewModel()
        {
            this.PlayerSettings = PlayerSettings.ReadSettings();
            this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.PlayerSettings.MainWindow);

            this.PlayerEngine.Configure(this.PlayerSettings);

            this.PlayListFileSearchWorker = new FileSearchWorker(MediaFile.GetMediaFileViewModel);
            this.MedialibFileSearchWorker = new FileSearchWorker(MediaFile.GetMediaFileViewModel);

            this.MedialibViewModel = new MedialibViewModel(this);
            this.PlayListsViewModel = new PlayListsViewModel(this);

            this.PlayControlInfoViewModel = new PlayControlInfoViewModel(this);

            this.ShutDownCommand = ReactiveCommand.CreateAsyncTask(x => this.ShutDown());

            this.PlayControlInfoViewModel.PlayControlViewModel.ShowMediaLibraryCommand.Subscribe(x => this.ShowMediaLibrary());

        }

        public CustomWindowPlacementSettings CustomWindowPlacementSettings { get; private set; }

        public PlayerEngine PlayerEngine
        {
            get { return PlayerEngine.Instance; }
        }

        public PlayerSettings PlayerSettings { get; private set; }

        public FileSearchWorker PlayListFileSearchWorker { get; private set; }

        public FileSearchWorker MedialibFileSearchWorker { get; private set; }

        public PlayControlInfoViewModel PlayControlInfoViewModel { get; private set; }

        public PlayListsViewModel PlayListsViewModel { get; private set; }

        public MedialibViewModel MedialibViewModel { get; private set; }

        public void ShowMediaLibrary()
        {
            if (this.medialibView != null)
            {
                this.medialibView.Activate();
            }
            else
            {
                this.medialibView = new MedialibView { ViewModel = this.MedialibViewModel };
                this.medialibView.Closed += (sender, args) => this.medialibView = null;
                this.medialibView.Show();
            }
        }

        public ICommand ShowOnGitHubCmd
        {
            get { return this.showOnGitHubCmd ?? (this.showOnGitHubCmd = new DelegateCommand(this.ShowOnGitHub, () => true)); }
        }

        private void ShowOnGitHub()
        {
            System.Diagnostics.Process.Start("https://github.com/punker76/simple-music-player");
        }

        public ReactiveCommand<Unit> ShutDownCommand { get; private set; }

        private async Task ShutDown()
        {
            foreach (var w in Application.Current.Windows.OfType<Window>())
            {
                w.Close();
            }
            this.PlayerSettings.WriteSettings();
            await this.PlayListsViewModel.SavePlayListAsync();
            this.PlayerEngine.CleanUp();
        }

        public bool HandleKeyDown(Key key)
        {
            if (this.PlayControlInfoViewModel.PlayControlViewModel.HandleKeyDown(key))
            {
                return true;
            }
            return false;
        }
    }
}