using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlaylistsViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private IEnumerable firstSimplePlaylistFiles;
    private IMediaFile selectedPlayListFile;
    private ICommand playCommand;
    private SMPSettings smpSettings;

    public PlaylistsViewModel(Dispatcher dispatcher, SMPSettings settings) {
      this.smpSettings = settings;
    }

    public async void HandleDropActionAsync(StringCollection fileOrDirDropList) {
      if (FileSearchWorker.Instance.CanStartSearch()) {
        var files = await FileSearchWorker.Instance.StartSearchAsync(fileOrDirDropList);
        this.PlayerEngine.Stop();
        this.FirstSimplePlaylistFiles = CollectionViewSource.GetDefaultView(new PlayListObservableCollection(files));
      }
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public IEnumerable FirstSimplePlaylistFiles {
      get { return this.firstSimplePlaylistFiles; }
      set {
        if (Equals(value, this.firstSimplePlaylistFiles)) {
          return;
        }
        this.firstSimplePlaylistFiles = value;
        this.OnPropertyChanged(() => this.FirstSimplePlaylistFiles);
      }
    }

    public IMediaFile SelectedPlayListFile {
      get { return this.selectedPlayListFile; }
      set {
        if (Equals(value, this.selectedPlayListFile)) {
          return;
        }
        this.selectedPlayListFile = value;
        this.OnPropertyChanged(() => this.SelectedPlayListFile);
      }
    }

    public ICommand PlayCommand {
      get { return this.playCommand ?? (this.playCommand = new DelegateCommand(this.Play, this.CanPlay)); }
    }

    private bool CanPlay() {
      return this.FirstSimplePlaylistFiles != null
             && this.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
      //&& (this.PlayerEngine.State == PlayerState.Pause || this.PlayerEngine.State == PlayerState.Stop);
    }

    private void Play() {
      var file = this.GetCurrentPlayListFile();
      if (file != null && this.SetCurrentPlayListFile(file)) {
        this.PlayerEngine.Play(file);
      }
    }

    public IMediaFile GetCurrentPlayListFile() {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        var currentFile = this.SelectedPlayListFile;// ?? fileCollView.CurrentItem;
        if (currentFile == null) {
          if (this.smpSettings.PlayerSettings.ShuffleMode) {
            return this.GetRandomPlayListFile();
          } else if (fileCollView.MoveCurrentToFirst()) {
            return fileCollView.CurrentItem as IMediaFile;
          }
        }
        return currentFile as IMediaFile;
      }
      return null;
    }

    private bool SetCurrentPlayListFile(IMediaFile file) {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        return fileCollView.MoveCurrentTo(file);
      }
      return false;
    }

    public IMediaFile GetPrevPlayListFile() {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        if (this.smpSettings.PlayerSettings.ShuffleMode) {
          return this.GetRandomPlayListFile();
        } else {
          if (fileCollView.MoveCurrentToPrevious() || fileCollView.MoveCurrentToLast()) {
            return fileCollView.CurrentItem as IMediaFile;
          }
        }
      }
      return null;
    }

    public IMediaFile GetNextPlayListFile() {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        if (this.smpSettings.PlayerSettings.ShuffleMode) {
          return this.GetRandomPlayListFile();
        } else {
          if (fileCollView.MoveCurrentToNext() || fileCollView.MoveCurrentToFirst()) {
            return fileCollView.CurrentItem as IMediaFile;
          }
        }
      }
      return null;
    }

    public IMediaFile GetRandomPlayListFile() {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        var count = fileCollView.SourceCollection.OfType<IMediaFile>().Count();
        var r = new Random(Environment.TickCount);
        var pos = r.Next(0, count - 1);
        if (pos == fileCollView.CurrentPosition) {
          while (pos == fileCollView.CurrentPosition) {
            pos = r.Next(0, count - 1);
          }
        }
        if (fileCollView.MoveCurrentToPosition(pos)) {
          return fileCollView.CurrentItem as IMediaFile;
        }
      }
      return null;

      /* new Random() already uses the current time. It is equivalent to new Random(Environment.TickCount).
       * But this is an implementation detail and might change in future versions of .net
       * I'd recommend using new Random() and only provide a fixed seed if you want to get a reproducible sequence of pseudo random values.
       * Since you need a known seed just use Environment.TickCount just like MS does. And then transmit it to the other program instances as seed.
       * If you create multiple instances of Random in a short interval (could be 16ms) they can be seeded to the same value,
       * and thus create the same pseudo-random sequence. But that's most likely not a problem here.
       * This common pitfall is caused by windows updating the current time(DateTime.Now/.UtcNow)
       * and the TickCount(Environment.TickCount) only every few milliseconds.
       * The exact interval depends on the version of windows and on what other programs are running.
       * Typical intervals where they don't change are 16ms or 1ms.
       */
    }
  }
}