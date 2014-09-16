using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReactiveUI;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class MedialibViewModel : ReactiveObject
  {
    public MedialibViewModel(Dispatcher dispatcher, SMPSettings settings) {
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(settings.MedialibSettings);
      this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(null));

      // Do a selection/filtering when nothing new has been changed for 400 ms and it isn't
      // an empty string... and don't filter for the same thing twice.

      this.ObservableForProperty(x => x.SelectedGenre)
        .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
        .Select(x => x.Value)
        //.Where(x => !string.IsNullOrWhiteSpace(x))
        .DistinctUntilChanged()
        .Subscribe(x => FilterByGenreSelection(x));

      this.ObservableForProperty(x => x.SelectedArtist)
        .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
        .Select(x => x.Value)
        //.Where(x => !string.IsNullOrWhiteSpace(x)/* && !string.IsNullOrEmpty(this.SelectedGenre)*/)
        .DistinctUntilChanged()
        .Subscribe(x => FilterByArtistSelection(this.SelectedGenre, x));

      this.ObservableForProperty(x => x.SelectedAlbum)
        .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
        .Select(x => x.Value)
        //.Where(x => !string.IsNullOrWhiteSpace(x)/* && !string.IsNullOrEmpty(this.SelectedGenre) && !string.IsNullOrEmpty(this.SelectedArtist)*/)
        .DistinctUntilChanged()
        .Subscribe(x => FilterByAlbumSelection());
    }

    private FileSearchWorker fileSearchWorker;

    public FileSearchWorker FileSearchWorker {
      get { return this.fileSearchWorker ?? (this.fileSearchWorker = new FileSearchWorker()); }
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings { get; set; }

    private IEnumerable mediaFiles;

    public IEnumerable MediaFiles {
      get { return this.mediaFiles; }
      set { this.RaiseAndSetIfChanged(ref mediaFiles, value); }
    }

    private IEnumerable genreList;

    public IEnumerable GenreList {
      get { return this.genreList; }
      set { this.RaiseAndSetIfChanged(ref genreList, value); }
    }

    private string selectedGenre;

    public string SelectedGenre {
      get { return this.selectedGenre; }
      set { this.RaiseAndSetIfChanged(ref selectedGenre, value); }
    }

    private IEnumerable artistList;

    public IEnumerable ArtistList {
      get { return this.artistList; }
      set { this.RaiseAndSetIfChanged(ref artistList, value); }
    }

    private string selectedArtist;

    public string SelectedArtist {
      get { return this.selectedArtist; }
      set { this.RaiseAndSetIfChanged(ref selectedArtist, value); }
    }

    private IEnumerable albumList;

    public IEnumerable AlbumList {
      get { return this.albumList; }
      set { this.RaiseAndSetIfChanged(ref albumList, value); }
    }

    private string selectedAlbum;

    public string SelectedAlbum {
      get { return this.selectedAlbum; }
      set { this.RaiseAndSetIfChanged(ref selectedAlbum, value); }
    }

    private ICommand addDirectoryCommand;

    public ICommand AddDirectoryCommand {
      get { return this.addDirectoryCommand ?? (this.addDirectoryCommand = new DelegateCommand(this.AddDirectory, () => true)); }
    }

    private void AddDirectory() {
      var owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.DataContext == this);
      var directories = FolderBrowserHelper.GetFolders(owner);
      if (directories.Any()) {
        this.HandleDropAction(directories.ToList());
      }
    }

    public async void HandleDropAction(IList fileOrDirDropList) {
      if (this.FileSearchWorker.CanStartSearch()) {
        var files = await this.FileSearchWorker.StartSearchAsync(fileOrDirDropList);

        var collView = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(files));
        collView.Filter = o => {
                            var m = (IMediaFile)o;
                            if (!string.IsNullOrWhiteSpace(this.SelectedGenre) && !Equals(m.FirstGenre, this.SelectedGenre)) {
                              return false;
                            }
                            if (!string.IsNullOrWhiteSpace(this.SelectedArtist) && !Equals(m.FirstPerformer, this.SelectedArtist)) {
                              return false;
                            }
                            if (!string.IsNullOrWhiteSpace(this.SelectedAlbum) && !Equals(m.Album, this.SelectedAlbum)) {
                              return false;
                            }
                            return true;
                          };
        this.MediaFiles = collView;

        this.GenreList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstGenre).OrderBy(g => g.Key).Select(g => g.Key));
        this.ArtistList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstPerformer).OrderBy(g => g.Key).Select(g => g.Key));
        this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));

        //        this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(files));
        //        ((ICollectionView)this.MediaFiles).GroupDescriptions.Add(new PropertyGroupDescription("Album"));
      }
    }

    public void FilterByGenreSelection(string genre) {
      var collView = this.MediaFiles as ICollectionView;
      if (collView == null) {
        return;
      }
      var files = collView.SourceCollection.OfType<IMediaFile>().ToList();
      if (!string.IsNullOrWhiteSpace(genre)) {
        files = files.Where(m => Equals(m.FirstGenre, genre)).ToList();
      }
      this.ArtistList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstPerformer).OrderBy(g => g.Key).Select(g => g.Key));
      this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));
      collView.Refresh();
    }

    public void FilterByArtistSelection(string genre, string artist) {
      var collView = this.MediaFiles as ICollectionView;
      if (collView == null) {
        return;
      }
      var files = collView.SourceCollection.OfType<IMediaFile>().ToList();
      if (!string.IsNullOrWhiteSpace(genre)) {
        files = files.Where(m => Equals(m.FirstGenre, genre)).ToList();
      }
      if (!string.IsNullOrWhiteSpace(artist)) {
        files = files.Where(m => Equals(m.FirstPerformer, artist)).ToList();
      }
      this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));
      collView.Refresh();
    }

    public void FilterByAlbumSelection() {
      var collView = this.MediaFiles as ICollectionView;
      if (collView == null) {
        return;
      }
      collView.Refresh();
    }
  }
}