using System.Collections;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class MedialibViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private IEnumerable mediaFiles;

    public MedialibViewModel(Dispatcher dispatcher) {
    }

    public async void HandleDropAction(StringCollection fileOrDirDropList) {
      if (FileSearchWorker.Instance.CanStartSearch()) {
        var files = await FileSearchWorker.Instance.StartSearchAsync(fileOrDirDropList);
        //this.PlayerEngine.Stop();
        this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibObservableCollection(files));
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