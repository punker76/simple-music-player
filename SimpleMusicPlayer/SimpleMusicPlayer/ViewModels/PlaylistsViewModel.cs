using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlaylistsViewModel : ViewModelBase, IDropTarget, IKeyHandler
  {
    private IEnumerable firstSimplePlaylistFiles;
    private IMediaFile selectedPlayListFile;
    private IEnumerable<IMediaFile> selectedPlayListFiles;
    private ICommand deleteCommand;
    private ICommand playCommand;
    private readonly SMPSettings smpSettings;
    private string playListItemTemplate;

    public PlaylistsViewModel(Dispatcher dispatcher, SMPSettings settings) {
      this.smpSettings = settings;
      this.SelectedPlayListFiles = new ObservableCollection<IMediaFile>();
      this.LoadPlayListAsync();
    }

    private FileSearchWorker fileSearchWorker;

    public FileSearchWorker FileSearchWorker {
      get { return this.fileSearchWorker ?? (this.fileSearchWorker = new FileSearchWorker()); }
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

    public IEnumerable<IMediaFile> SelectedPlayListFiles {
      get { return this.selectedPlayListFiles; }
      set {
        if (Equals(value, this.selectedPlayListFiles)) {
          return;
        }
        this.selectedPlayListFiles = value;
        this.OnPropertyChanged(() => this.SelectedPlayListFiles);
      }
    }

    public ICommand DeleteCommand {
      get { return this.deleteCommand ?? (this.deleteCommand = new DelegateCommand(this.DeleteSelectedFiles, this.CanDeleteSelectedFiles)); }
    }

    private bool CanDeleteSelectedFiles() {
      return this.FirstSimplePlaylistFiles != null
             && this.SelectedPlayListFiles != null
             && this.SelectedPlayListFiles.Any();
    }

    private void DeleteSelectedFiles() {
      var filesCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (filesCollView != null) {
        var currentPlayingFile = filesCollView.CurrentItem as IMediaFile;
        var filesColl = ((IList)filesCollView.SourceCollection);
        var files2Delete = this.SelectedPlayListFiles.ToList();
        foreach (var mediaFile in files2Delete) {
          filesColl.Remove(mediaFile);
        }
        if (currentPlayingFile != null && files2Delete.Contains(currentPlayingFile)) {
          // for the first time go to nothing
          filesCollView.MoveCurrentTo(null);
        }
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
        case Key.Delete:
          handled = this.DeleteCommand.CanExecute(null);
          if (handled) {
            this.DeleteCommand.Execute(null);
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
        dropInfo.Effects = this.FileSearchWorker.CanStartSearch() ? DragDropEffects.Copy : DragDropEffects.None;
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

    private async void HandleDropActionAsync(IDropInfo dropInfo, IList fileOrDirDropList) {
      if (this.FileSearchWorker.CanStartSearch()) {
        var files = await this.FileSearchWorker.StartSearchAsync(fileOrDirDropList);

        var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;

        if (currentFilesCollView == null) {
          var filesColl = new PlayListObservableCollection(files);
          var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
          this.FirstSimplePlaylistFiles = filesCollView;
          ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);
        } else {
          var insertIndex = dropInfo.InsertIndex;
          var destinationList = DefaultDropHandler.GetList(dropInfo.TargetCollection);
          foreach (var o in files) {
            destinationList.Insert(insertIndex++, o);
          }

          var mediaFiles = destinationList.OfType<IMediaFile>().ToList();
          this.ResetPlayListIndices(mediaFiles);
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

    public void SavePlayList() {
      var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;
      if (currentFilesCollView != null) {
        PlayList.SavePlayListAsync(currentFilesCollView.SourceCollection);
      }
    }

    public string PlayListItemTemplate
    {
      get { return this.playListItemTemplate; }
      set
      {
        if (Equals(value, this.playListItemTemplate))
        {
          return;
        }
        this.playListItemTemplate = value;
        this.OnPropertyChanged(() => this.PlayListItemTemplate);
      }
    }

    public void CalcPlayListItemTemplateByActualWidth(double actualWidth)
    {
      if (actualWidth > 850) PlayListItemTemplate = "Large";
      else if (actualWidth > 560) PlayListItemTemplate = "Medium";
      else PlayListItemTemplate = "Small";
    }
  }
}