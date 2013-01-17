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
    private readonly PlaylistsViewModel playlistsViewModel;
    private ICommand playOrPauseCommand;
    private ICommand stopCommand;
    private ICommand playPrevCommand;
    private ICommand playNextCommand;
    private ICommand shuffleCommand;
    private ICommand repeatCommand;
    private SMPSettings smpSettings;

    public PlayControlViewModel(Dispatcher dispatcher, SMPSettings settings, PlaylistsViewModel playlistsViewModel) {
      this.playlistsViewModel = playlistsViewModel;

      this.SMPSettings = settings;

      this.PlayerEngine.PlayNextFileAction = () => {
        if (this.SMPSettings.PlayerSettings.RepeatMode) {
          if (this.CanPlayOrPause()) {
            var file = this.playlistsViewModel.GetCurrentPlayListFile();
            if (file != null) {
              this.PlayerEngine.Play(file);
            }
          }
        } else {
          if (this.CanPlayNext()) {
            this.PlayNext();
          }
        }
      };
    }

    public SMPSettings SMPSettings {
      get { return this.smpSettings; }
      private set {
        if (Equals(value, this.smpSettings)) {
          return;
        }
        this.smpSettings = value;
        this.OnPropertyChanged(() => this.SMPSettings);
      }
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public ICommand PlayOrPauseCommand {
      get { return this.playOrPauseCommand ?? (this.playOrPauseCommand = new DelegateCommand(this.PlayOrPause, this.CanPlayOrPause)); }
    }

    private bool CanPlayOrPause() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayOrPause() {
      if (this.PlayerEngine.State == PlayerState.Pause || this.PlayerEngine.State == PlayerState.Play) {
        this.PlayerEngine.Pause();
      } else {
        var file = this.playlistsViewModel.GetCurrentPlayListFile();
        if (file != null) {
          this.PlayerEngine.Play(file);
        }
      }
    }

    public ICommand StopCommand {
      get { return this.stopCommand ?? (this.stopCommand = new DelegateCommand(this.Stop, this.CanStop)); }
    }

    private bool CanStop() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void Stop() {
      this.PlayerEngine.Stop();
    }

    public ICommand PlayPrevCommand {
      get { return this.playPrevCommand ?? (this.playPrevCommand = new DelegateCommand(this.PlayPrev, this.CanPlayPrev)); }
    }

    private bool CanPlayPrev() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayPrev() {
      var file = this.playlistsViewModel.GetPrevPlayListFile();
      if (file != null) {
        this.PlayerEngine.Play(file);
      }
    }

    public ICommand PlayNextCommand {
      get { return this.playNextCommand ?? (this.playNextCommand = new DelegateCommand(this.PlayNext, this.CanPlayNext)); }
    }

    private bool CanPlayNext() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
    }

    private void PlayNext() {
      var file = this.playlistsViewModel.GetNextPlayListFile();
      if (file != null) {
        this.PlayerEngine.Play(file);
      }
    }

    public ICommand ShuffleCommand {
      get { return this.shuffleCommand ?? (this.shuffleCommand = new DelegateCommand(this.SetShuffelMode, this.CanSetShuffelMode)); }
    }

    private bool CanSetShuffelMode() {
      return this.PlayerEngine.Initializied;
    }

    private void SetShuffelMode() {
      this.SMPSettings.PlayerSettings.ShuffleMode = !this.SMPSettings.PlayerSettings.ShuffleMode;
    }

    public ICommand RepeatCommand {
      get { return this.repeatCommand ?? (this.repeatCommand = new DelegateCommand(this.SetRepeatMode, this.CanSetRepeatMode)); }
    }

    public bool CanSetRepeatMode() {
      return this.PlayerEngine.Initializied;
    }

    public void SetRepeatMode() {
      this.SMPSettings.PlayerSettings.RepeatMode = !this.SMPSettings.PlayerSettings.RepeatMode;
    }

    public bool HandleKeyDown(Key key) {
      var handled = false;
      switch (key) {
        case Key.R:
          handled = this.RepeatCommand.CanExecute(null);
          if (handled) {
            this.RepeatCommand.Execute(null);
          }
          break;
        case Key.S:
          handled = this.ShuffleCommand.CanExecute(null);
          if (handled) {
            this.ShuffleCommand.Execute(null);
          }
          break;
        case Key.J:
          handled = this.PlayNextCommand.CanExecute(null);
          if (handled) {
            this.PlayNextCommand.Execute(null);
          }
          break;
        case Key.K:
          handled = this.PlayPrevCommand.CanExecute(null);
          if (handled) {
            this.PlayPrevCommand.Execute(null);
          }
          break;
        case Key.Space:
          handled = this.PlayOrPauseCommand.CanExecute(null);
          if (handled) {
            this.PlayOrPauseCommand.Execute(null);
          }
          break;
      }
      return handled;
    }
  }
}