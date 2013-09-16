using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using Application = System.Windows.Application;

namespace SimpleMusicPlayer.ViewModels
{
  public class MedialibViewModel : ViewModelBase
  {
    private IEnumerable mediaFiles;
    private CustomWindowPlacementSettings customWindowPlacementSettings;

    public MedialibViewModel(Dispatcher dispatcher, SMPSettings settings) {
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(settings.MedialibSettings);
      this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(null));
    }

    private FileSearchWorker fileSearchWorker;

    public FileSearchWorker FileSearchWorker {
      get { return this.fileSearchWorker ?? (this.fileSearchWorker = new FileSearchWorker()); }
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings {
      get { return this.customWindowPlacementSettings; }
      set {
        if (Equals(value, this.customWindowPlacementSettings)) {
          return;
        }
        this.customWindowPlacementSettings = value;
        this.OnPropertyChanged(() => this.CustomWindowPlacementSettings);
      }
    }

    public IEnumerable MediaFiles {
      get { return this.mediaFiles; }
      set {
        if (Equals(value, this.mediaFiles)) {
          return;
        }
        this.mediaFiles = value;
        this.OnPropertyChanged(() => this.MediaFiles);
      }
    }

    private IEnumerable genreList;

    public IEnumerable GenreList {
      get { return this.genreList; }
      set {
        if (Equals(value, this.genreList)) {
          return;
        }
        this.genreList = value;
        this.OnPropertyChanged(() => this.GenreList);
      }
    }

    private string selectedGenre;

    public string SelectedGenre {
      get { return this.selectedGenre; }
      set {
        if (Equals(value, this.selectedGenre)) {
          return;
        }
        this.selectedGenre = value;
        this.OnPropertyChanged(() => this.SelectedGenre);
      }
    }

    private IEnumerable artistList;

    public IEnumerable ArtistList {
      get { return this.artistList; }
      set {
        if (Equals(value, this.artistList)) {
          return;
        }
        this.artistList = value;
        this.OnPropertyChanged(() => this.ArtistList);
      }
    }

    private string selectedArtist;

    public string SelectedArtist {
      get { return this.selectedArtist; }
      set {
        if (Equals(value, this.selectedArtist)) {
          return;
        }
        this.selectedArtist = value;
        this.OnPropertyChanged(() => this.SelectedArtist);
      }
    }

    private IEnumerable albumList;

    public IEnumerable AlbumList {
      get { return this.albumList; }
      set {
        if (Equals(value, this.albumList)) {
          return;
        }
        this.albumList = value;
        this.OnPropertyChanged(() => this.AlbumList);
      }
    }

    private string selectedAlbum;

    public string SelectedAlbum {
      get { return this.selectedAlbum; }
      set {
        if (Equals(value, this.selectedAlbum)) {
          return;
        }
        this.selectedAlbum = value;
        this.OnPropertyChanged(() => this.SelectedAlbum);
      }
    }

    private ICommand addDirectoryCommand;

    public ICommand AddDirectoryCommand {
      get { return this.addDirectoryCommand ?? (this.addDirectoryCommand = new DelegateCommand(this.AddDirectory, () => true)); }
    }

    private void AddDirectory() {
      var owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.DataContext == this);
      var directory = string.Empty;
      if (CommonFileDialog.IsPlatformSupported) {
        using (var dialog = new CommonOpenFileDialog()) {
          dialog.IsFolderPicker = true;
          var result = dialog.ShowDialog(owner);
          if (result == CommonFileDialogResult.Ok) {
            directory = dialog.FileName;
          }
        }
      } else {
        using (var dialog = new FolderBrowserDialog()) {
          var result = owner is IWin32Window
                         ? dialog.ShowDialog((IWin32Window)owner)
                         : dialog.ShowDialog();
          if (result == DialogResult.OK) {
            directory = dialog.SelectedPath;
          }
        }
      }
      if (!string.IsNullOrWhiteSpace(directory)) {
        this.HandleDropAction(new[] { directory });
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

    private ICommand filterByGenreSelectionCommand;

    public ICommand FilterByGenreSelectionCommand {
      get {
        return this.filterByGenreSelectionCommand ?? (this.filterByGenreSelectionCommand =
                                                      new DelegateCommand(() => this.FilterByGenreSelection(this.SelectedGenre),
                                                                          () => !string.IsNullOrWhiteSpace(this.SelectedGenre)));
      }
    }

    private ICommand filterByArtistSelectionCommand;

    public ICommand FilterByArtistSelectionCommand {
      get {
        return this.filterByArtistSelectionCommand ?? (this.filterByArtistSelectionCommand =
                                                       new DelegateCommand(() => this.FilterByArtistSelection(this.SelectedGenre, this.SelectedArtist),
                                                                           () => !string.IsNullOrWhiteSpace(this.SelectedArtist)));
      }
    }

    private ICommand filterByAlbumSelectionCommand;

    public ICommand FilterByAlbumSelectionCommand {
      get {
        return this.filterByAlbumSelectionCommand ?? (this.filterByAlbumSelectionCommand =
                                                      new DelegateCommand(() => this.FilterByAlbumSelection(this.SelectedGenre, this.SelectedArtist, this.SelectedAlbum),
                                                                          () => !string.IsNullOrWhiteSpace(this.SelectedAlbum)));
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

    public void FilterByAlbumSelection(string genre, string artist, string album) {
      var collView = this.MediaFiles as ICollectionView;
      if (collView == null) {
        return;
      }
      collView.Refresh();
    }
  }
}