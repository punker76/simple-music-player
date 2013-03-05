using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlayInfoViewModel : ViewModelBase
  {
    public PlayInfoViewModel(Dispatcher dispatcher) {
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }
  }
}