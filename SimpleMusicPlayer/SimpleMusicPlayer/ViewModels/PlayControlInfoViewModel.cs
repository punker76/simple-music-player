using System.Windows.Threading;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Player;

namespace SimpleMusicPlayer.ViewModels
{
    public class PlayControlInfoViewModel : ViewModelBase
    {
        public PlayControlInfoViewModel(MainViewModel mainViewModel)
        {
            this.PlayerEngine = mainViewModel.PlayerEngine;
            this.PlayControlViewModel = new PlayControlViewModel(mainViewModel);
        }

        public PlayerEngine PlayerEngine { get; private set; }

        public PlayControlViewModel PlayControlViewModel { get; private set; }
    }
}
