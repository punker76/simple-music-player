using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SimpleMusicPlayer.Base
{
  public class QuickFillObservableCollection<T> : ObservableCollection<T>
  {
    private bool suspendCollectionChanged;

    public QuickFillObservableCollection() {
    }

    public QuickFillObservableCollection(IEnumerable<T> collection)
      : this() {
      this.Fill(collection);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if (!this.suspendCollectionChanged) {
        base.OnCollectionChanged(e);
      }
    }

    public void Fill(IEnumerable<T> collection) {
      if (collection == null) {
        throw new ArgumentNullException("collection");
      }
      var newItems = collection.ToArray();
      if (newItems.Length == 1) {
        this.Add(newItems.First());
      } else if (newItems.Length > 0) {
        this.suspendCollectionChanged = true;
        try {
          foreach (var item in newItems) {
            this.InsertItem(this.Count, item);
          }
        } finally {
          this.suspendCollectionChanged = false;
          this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
      }
    }
  }
}