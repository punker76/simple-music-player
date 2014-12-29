using SimpleMusicPlayer.Core;

namespace SimpleMusicPlayer.ViewModels
{
    public class PlayControlInfoViewModel : ViewModelBase
    {
        public PlayControlInfoViewModel(MainViewModel mainViewModel)
        {
            this.PlayControlViewModel = new PlayControlViewModel(mainViewModel);
        }

        public PlayControlViewModel PlayControlViewModel { get; private set; }
    }
}
