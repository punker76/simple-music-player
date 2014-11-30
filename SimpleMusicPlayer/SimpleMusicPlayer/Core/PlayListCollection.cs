using System.Collections.Generic;
using System.Collections.Specialized;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core
{
    public class PlayListCollection : QuickFillObservableCollection<IMediaFile>
    {
        public PlayListCollection(IEnumerable<IMediaFile> files)
            : base(files)
        {
        }

        protected override void InsertItem(int index, IMediaFile item)
        {
            //item.PlayListIndex = index + 1;
            //item.PlayList = this;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            //var item = this[index];
            //item.PlayListIndex = -1;
            //item.PlayList = null;
            base.RemoveItem(index);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            //var item = this[newIndex];
            //item.PlayListIndex = newIndex + 1;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Recommended is to avoid reentry 
            // in collection changed event while collection
            // is getting changed on other thread.
            using (this.BlockReentrancy())
            {
                if (!this.suspendCollectionChangeNotification)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        var newIndex = e.NewStartingIndex;
                        for (var idx = newIndex; idx < this.Count; idx++)
                        {
                            this[idx].PlayListIndex = idx + 1;
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        var newIndex = e.OldStartingIndex;
                        for (var idx = newIndex; idx < this.Count; idx++)
                        {
                            this[idx].PlayListIndex = idx + 1;
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        for (var idx = 0; idx < this.Count; idx++)
                        {
                            this[idx].PlayListIndex = idx + 1;
                        }
                    }
                }
            }
            base.OnCollectionChanged(e);
        }
    }
}