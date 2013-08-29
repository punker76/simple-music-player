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

    public MedialibViewModel(Dispatcher dispatcher) {
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
  }
}