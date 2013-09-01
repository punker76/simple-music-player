using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

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

    public async void HandleDropAction(StringCollection fileOrDirDropList) {
      if (FileSearchWorker.Instance.CanStartSearch()) {
        var files = await FileSearchWorker.Instance.StartSearchAsync(fileOrDirDropList);
        //this.PlayerEngine.Stop();
        this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(files));
        ((ICollectionView)this.MediaFiles).GroupDescriptions.Add(new PropertyGroupDescription("Album"));
      }
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
  }
}