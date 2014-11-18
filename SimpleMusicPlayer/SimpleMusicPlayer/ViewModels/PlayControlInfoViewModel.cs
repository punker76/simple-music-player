using SimpleMusicPlayer.Base;
using System.Windows.Threading;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
    public class PlayControlInfoViewModel : ViewModelBase
    {
        public PlayControlInfoViewModel(Dispatcher dispatcher, MainViewModel mainViewModel)
        {
            this.PlayerEngine = mainViewModel.PlayerEngine;
            this.PlayControlViewModel = new PlayControlViewModel(dispatcher, mainViewModel);
        }

        public PlayerEngine PlayerEngine { get; private set; }

        public PlayControlViewModel PlayControlViewModel { get; private set; }
    }
}
