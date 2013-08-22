using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.Models;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlaylistsViewModel : ViewModelBase, IDropTarget
  {
    private IEnumerable firstSimplePlaylistFiles;
    private IMediaFile selectedPlayListFile;
    private ICommand playCommand;
    private readonly SMPSettings smpSettings;

    public PlaylistsViewModel(Dispatcher dispatcher, SMPSettings settings) {
      this.smpSettings = settings;

      this.LoadPlayListAsync();
    }

    public async void LoadPlayListAsync() {
      var playList = await PlayList.GetPlayListAsync();
      if (playList != null) {
        var filesColl = new PlayListObservableCollection(playList.Files);
        var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
        this.FirstSimplePlaylistFiles = filesCollView;
        ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);

        //        await Task.Factory
        //                  .StartNew(() => {
        //                    return filesCollView;
        //                  })
        //                  .ContinueWith(task => {
        //                  },
        //                                CancellationToken.None,
        //                                TaskContinuationOptions.LongRunning,
        //                                TaskScheduler.FromCurrentSynchronizationContext());
      }
    }

    public async void SavePlayListAsync(IEnumerable<IMediaFile> files) {
      var pl = new PlayList();
      pl.Files = files.OfType<MediaFile>().ToList();
      var playListAsJson = await JsonConvert.SerializeObjectAsync(pl, Formatting.None);
      await Task.Factory.StartNew(() => File.WriteAllText(PlayList.PlayListFileName, playListAsJson));
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
    }

    private void Play() {
      var file = this.SelectedPlayListFile;
      if (file != null && this.SetCurrentPlayListFile(file)) {
        this.PlayerEngine.Play(file);
      }
    }

    public IMediaFile GetCurrentPlayListFile() {
      var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (fileCollView != null) {
        var currentFile = this.smpSettings.PlayerSettings.RepeatMode ? fileCollView.CurrentItem : (this.SelectedPlayListFile ?? fileCollView.CurrentItem);
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
      if (file != null) {
        var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
        if (fileCollView != null) {
          return fileCollView.MoveCurrentTo(file);
        }
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

    public bool HandleKeyDown(Key key) {
      var handled = false;
      switch (key) {
        case Key.Enter:
          handled = this.PlayCommand.CanExecute(null);
          if (handled) {
            this.PlayCommand.Execute(null);
          }
          break;
      }
      return handled;
    }

    public void DragOver(IDropInfo dropInfo) {
      dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

      var dataObject = dropInfo.Data as IDataObject;
      // look for drag&drop new files
      if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop)) {
        dropInfo.Effects = FileSearchWorker.Instance.CanStartSearch() ? DragDropEffects.Copy : DragDropEffects.None;
      } else {
        dropInfo.Effects = DragDropEffects.Move;
      }
    }

    public void Drop(IDropInfo dropInfo) {
      var dataObject = dropInfo.Data as DataObject;
      // look for drag&drop new files
      if (dataObject != null && dataObject.ContainsFileDropList()) {
        this.HandleDropActionAsync(dropInfo, dataObject.GetFileDropList());
      } else {
        GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        this.ResetPlayListIndices(dropInfo.TargetCollection.OfType<IMediaFile>());
        var mediaFile = dropInfo.Data as IMediaFile;
        if (mediaFile != null && mediaFile.State != PlayerState.Stop) {
          this.SetCurrentPlayListFile(mediaFile);
        }
      }
    }

    private async void HandleDropActionAsync(IDropInfo dropInfo, StringCollection fileOrDirDropList) {
      if (FileSearchWorker.Instance.CanStartSearch()) {
        var files = await FileSearchWorker.Instance.StartSearchAsync(fileOrDirDropList);

        var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;

        if (currentFilesCollView == null) {
          var filesColl = new PlayListObservableCollection(files);
          var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
          this.FirstSimplePlaylistFiles = filesCollView;
          ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);

          this.SavePlayListAsync(files);
        } else {
          var insertIndex = dropInfo.InsertIndex;
          var destinationList = DefaultDropHandler.GetList(dropInfo.TargetCollection);
          foreach (var o in files) {
            destinationList.Insert(insertIndex++, o);
          }

          var mediaFiles = destinationList.OfType<IMediaFile>().ToList();
          this.ResetPlayListIndices(mediaFiles);
          this.SavePlayListAsync(mediaFiles);
        }
      }
    }

    private void ResetPlayListIndices(IEnumerable<IMediaFile> mediaFiles) {
      // it's not the best but it works for the first time
      var i = 1;
      foreach (var mf in mediaFiles) {
        mf.PlayListIndex = i++;
      }
    }
  }
}