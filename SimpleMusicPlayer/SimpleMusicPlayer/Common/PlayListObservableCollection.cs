using System.Collections.Generic;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class PlayListObservableCollection : QuickFillObservableCollection<IMediaFile>
  {
    public PlayListObservableCollection(IEnumerable<IMediaFile> files)
      : base(files) {
    }

    protected override void InsertItem(int index, IMediaFile item) {
      item.PlayListIndex = index + 1;
      item.PlayList = this;
      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index) {
      IMediaFile item = this[index];
      item.PlayListIndex = -1;
      item.PlayList = null;
      base.RemoveItem(index);
    }
  }
}