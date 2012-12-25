using System.Collections.Generic;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class MedialibObservableCollection : QuickFillObservableCollection<IMediaFile>
  {
    public MedialibObservableCollection(IEnumerable<IMediaFile> files)
      : base(files) {
    }
  }
}