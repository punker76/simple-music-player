using System;
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
    private ICommand playCommand;
    private ICommand pauseCommand;
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
          if (this.CanPlay()) {
            this.Play();
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

    public ICommand PlayCommand {
      get { return this.playCommand ?? (this.playCommand = new DelegateCommand(this.Play, this.CanPlay)); }
    }

    private bool CanPlay() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
      //&& (this.PlayerEngine.State == PlayerState.Pause || this.PlayerEngine.State == PlayerState.Stop);
    }

    private void Play() {
      if (this.PlayerEngine.State == PlayerState.Pause) {
        this.PlayerEngine.Pause();
      } else {
        var file = this.playlistsViewModel.GetCurrentPlayListFile();
        if (file != null) {
          this.PlayerEngine.Play(file);
        }
      }
    }

    public ICommand PauseCommand {
      get { return this.pauseCommand ?? (this.pauseCommand = new DelegateCommand(this.Pause, this.CanPause)); }
    }

    private bool CanPause() {
      return this.PlayerEngine.Initializied
             && this.playlistsViewModel.FirstSimplePlaylistFiles != null
             && this.playlistsViewModel.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
      //&& this.PlayerEngine.State == PlayerState.Play;
    }

    private void Pause() {
      this.PlayerEngine.Pause();
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

    private bool CanSetRepeatMode() {
      return this.PlayerEngine.Initializied;
    }

    private void SetRepeatMode() {
      this.SMPSettings.PlayerSettings.RepeatMode = !this.SMPSettings.PlayerSettings.RepeatMode;
    }
  }
}