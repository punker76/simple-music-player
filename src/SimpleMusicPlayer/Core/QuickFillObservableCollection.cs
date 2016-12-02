using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;

namespace SimpleMusicPlayer.Core
{
    public class QuickFillObservableCollection<T> : ObservableCollection<T>
    {
        private readonly object locker = new object();
        protected bool suspendCollectionChangeNotification;

        public QuickFillObservableCollection()
        {
        }

        public QuickFillObservableCollection(IEnumerable<T> collection)
            : this()
        {
            this.AddItems(collection);
        }

        /// <summary>
        /// This method adds the given generic list of items as a range into current collection by casting them as type T.
        /// It then notifies once after all items are added.
        /// </summary>
        /// <param name="sourceItems">The source collection.</param>
        /// <param name="atIndex">The start index, where the items are inserted.</param>
        public void AddItems(IEnumerable<T> sourceItems, int atIndex = -1)
        {
            lock (this.locker)
            {
                if (sourceItems == null)
                {
                    return;
                }
                var enumerator = sourceItems.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return;
                }
                this.SuspendCollectionChangeNotification();
                try
                {
                    atIndex = atIndex < 0 ? this.Count : atIndex;
                    this.InsertItem(atIndex++, enumerator.Current);
                    while (enumerator.MoveNext())
                    {
                        this.InsertItem(atIndex++, enumerator.Current);
                    }
                }
                finally
                {
                    this.NotifyChanges();
                }
            }
        }

        /// <summary>
        /// This method removes the given generic list of items as a range into current collection by casting them as type T.
        /// It then notifies once after all items are removed.
        /// </summary>
        /// <param name="sourceItems">The source collection.</param>
        public void RemoveItems(IEnumerable<T> sourceItems)
        {
            lock (this.locker)
            {
                if (sourceItems == null)
                {
                    return;
                }
                var enumerator = sourceItems.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return;
                }
                this.SuspendCollectionChangeNotification();
                try
                {
                    this.Remove(enumerator.Current);
                    while (enumerator.MoveNext())
                    {
                        this.Remove(enumerator.Current);
                    }
                }
                finally
                {
                    this.NotifyChanges();
                }
            }
        }

        /// <summary>
        /// This event is overriden CollectionChanged event of the observable collection.
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises collection change event.
        /// </summary>
        public void NotifyChanges()
        {
            this.ResumeCollectionChangeNotification();
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(arg);
        }

        /// <summary>
        /// Resumes collection changed notification.
        /// </summary>
        public void ResumeCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Suspends collection changed notification.
        /// </summary>
        public void SuspendCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = true;
        }

        /// <summary>
        /// This collection changed event performs thread safe event raising.
        /// </summary>
        /// <param name="e">The event argument.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Recommended is to avoid reentry 
            // in collection changed event while collection
            // is getting changed on other thread.
            using (this.BlockReentrancy())
            {
                if (!this.suspendCollectionChangeNotification)
                {
                    var eventHandler = this.CollectionChanged;
                    if (eventHandler == null)
                    {
                        return;
                    }

                    // Walk thru invocation list.
                    var delegates = eventHandler.GetInvocationList().OfType<NotifyCollectionChangedEventHandler>();

                    foreach (var handler in delegates)
                    {
                        // If the subscriber is a DispatcherObject and different thread.
                        var dispatcherObject = handler.Target as DispatcherObject;

                        if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                        {
                            // Invoke handler in the target dispatcher's thread... 
                            // asynchronously for better responsiveness.
                            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                        }
                        else
                        {
                            // Execute handler as is.
                            handler(this, e);
                        }
                    }
                }
            }
        }
    }
}