using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlayControlViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private PlaylistsViewModel playlistsViewModel;
    private ICommand playCommand;
    private ICommand playPrevCommand;
    private ICommand playNextCommand;

    public PlayControlViewModel(Dispatcher dispatcher, PlaylistsViewModel playlistsViewModel) {
      this.playlistsViewModel = playlistsViewModel;
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public ICommand PlayCommand {
      get { return this.playCommand ?? (this.playCommand = new DelegateCommand(this.Play, this.CanPlay)); }
    }

    private bool CanPlay() {
      return this.playlistsViewModel.FirstSimplePlaylistFiles != null && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void Play() {
      var file = this.playlistsViewModel.SelectedPlayListFile;
      if (file != null) {
        PlayerEngine.Instance.Play(file);
      }
    }

    public ICommand PlayPrevCommand {
      get { return this.playPrevCommand ?? (this.playPrevCommand = new DelegateCommand(this.PlayPrev, this.CanPlayPrev)); }
    }

    private bool CanPlayPrev() {
      return this.playlistsViewModel.FirstSimplePlaylistFiles != null && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayPrev() {
      var file = this.playlistsViewModel.GetPrevPlayListFile();
      if (file != null) {
        PlayerEngine.Instance.Play(file);
      }
    }

    public ICommand PlayNextCommand {
      get { return this.playNextCommand ?? (this.playNextCommand = new DelegateCommand(this.PlayNext, this.CanPlayNext)); }
    }

    private bool CanPlayNext() {
      return this.playlistsViewModel.FirstSimplePlaylistFiles != null && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayNext() {
      var file = this.playlistsViewModel.GetNextPlayListFile();
      if (file != null) {
        PlayerEngine.Instance.Play(file);
      }
    }
  }
}