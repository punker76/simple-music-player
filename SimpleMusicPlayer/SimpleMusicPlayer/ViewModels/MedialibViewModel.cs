using System.Collections.Specialized;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class MedialibViewModel : ViewModelBaseNotifyPropertyChanged
  {
    public MedialibViewModel(Dispatcher dispatcher) {
    }

    public void HandleDropAction(StringCollection fileOrDirDropList) {
      //var files = fileOrDirDropList.GetFilesFromDropList();
    }
  }
}