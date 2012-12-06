using System.Windows.Threading;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer.ViewModels
{
  public class MainWindowViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private PlayControlViewModel playControlViewModel;
    private PlaylistsViewModel playlistsViewModel;
    private MedialibViewModel medialibViewModel;

    public MainWindowViewModel(Dispatcher dispatcher) {
      this.PlayControlViewModel = new PlayControlViewModel(dispatcher);
      this.PlaylistsViewModel = new PlaylistsViewModel(dispatcher);
      this.MedialibViewModel = new MedialibViewModel(dispatcher);
    }

    public PlayControlViewModel PlayControlViewModel {
      get { return this.playControlViewModel; }
      set {
        if (Equals(value, this.playControlViewModel)) {
          return;
        }
        this.playControlViewModel = value;
        this.OnPropertyChanged(() => this.PlayControlViewModel);
      }
    }

    public PlaylistsViewModel PlaylistsViewModel {
      get { return this.playlistsViewModel; }
      set {
        if (Equals(value, this.playlistsViewModel)) {
          return;
        }
        this.playlistsViewModel = value;
        this.OnPropertyChanged(() => this.PlaylistsViewModel);
      }
    }

    public MedialibViewModel MedialibViewModel {
      get { return this.medialibViewModel; }
      set {
        if (Equals(value, this.medialibViewModel)) {
          return;
        }
        this.medialibViewModel = value;
        this.OnPropertyChanged(() => this.MedialibViewModel);
      }
    }
  }
}