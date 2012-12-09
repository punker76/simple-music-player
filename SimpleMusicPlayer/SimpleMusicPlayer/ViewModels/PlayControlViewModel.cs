using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlayControlViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private PlaylistsViewModel playlistsViewModel;

    public PlayControlViewModel(Dispatcher dispatcher, PlaylistsViewModel playlistsViewModel) {
      this.playlistsViewModel = playlistsViewModel;
    }

    private ICommand playCommand;
    public ICommand PlayCommand {
      get { return this.playCommand ?? (this.playCommand = new DelegateCommand(this.Play, this.CanPlay)); }
    }

    private bool CanPlay() {
      return true;
    }

    private void Play() {
      var file = this.playlistsViewModel.SelectedPlayListFile;
      if (file != null) {
        PlayerEngine.Instance.Play(file);
      }
    }
  }
}