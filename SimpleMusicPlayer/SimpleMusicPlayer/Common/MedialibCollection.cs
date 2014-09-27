using System.Collections.Generic;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class MedialibCollection : QuickFillObservableCollection<IMediaFile>
  {
    public MedialibCollection(IEnumerable<IMediaFile> files)
      : base(files) {
    }
  }
}