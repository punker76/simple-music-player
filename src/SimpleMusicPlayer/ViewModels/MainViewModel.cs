using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.Core.Player;
using TinyIoC;

namespace SimpleMusicPlayer.ViewModels
{
    public class MainViewModel : ReactiveObject, IKeyHandler
    {
        public MainViewModel()
        {
            var container = TinyIoCContainer.Current;

            this.PlayerSettings = container.Resolve<PlayerSettings>().Update();

            this.PlayerEngine = container.Resolve<PlayerEngine>().Configure();

            this.PlayListsViewModel = new PlayListsViewModel();

            this.PlayControlInfoViewModel = new PlayControlInfoViewModel(this);

            this.ShowOnGitHubCmd = new DelegateCommand(this.ShowOnGitHub, () => true);
        }

        public PlayerEngine PlayerEngine { get; private set; }

        public PlayerSettings PlayerSettings { get; private set; }

        public CustomWindowPlacementSettings WindowPlacementSettings { get; set; }

        public PlayControlInfoViewModel PlayControlInfoViewModel { get; private set; }

        public PlayListsViewModel PlayListsViewModel { get; private set; }

        public ICommand ShowOnGitHubCmd { get; }

        private void ShowOnGitHub()
        {
            System.Diagnostics.Process.Start("https://github.com/punker76/simple-music-player");
        }

        public void ShutDown()
        {
            foreach (var w in Application.Current.Windows.OfType<Window>())
            {
                w.Close();
            }
            this.PlayerSettings.Save();
            this.PlayerEngine.CleanUp();
            this.PlayListsViewModel.SavePlayList();
        }

        public bool HandleKeyDown(KeyEventArgs args)
        {
            if (this.PlayControlInfoViewModel.PlayControlViewModel.HandleKeyDown(args))
            {
                return true;
            }
            return false;
        }
    }
}