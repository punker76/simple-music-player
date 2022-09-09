using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ReactiveUI;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public class BaseListBox : ListBox, IEnableLogger
    {
        public static readonly DependencyProperty ObserveItemContainerGeneratorProperty
            = DependencyProperty.Register(nameof(ObserveItemContainerGenerator),
                typeof(bool),
                typeof(BaseListBox),
                new PropertyMetadata(default(bool)));

        public bool ObserveItemContainerGenerator
        {
            get => (bool)this.GetValue(ObserveItemContainerGeneratorProperty);
            set => this.SetValue(ObserveItemContainerGeneratorProperty, value);
        }

        public static readonly DependencyProperty ScrollIndexProperty
            = DependencyProperty.Register(nameof(ScrollIndex),
                typeof(int),
                typeof(BaseListBox),
                new PropertyMetadata(-1));

        public int ScrollIndex
        {
            get => (int)this.GetValue(ScrollIndexProperty);
            set => this.SetValue(ScrollIndexProperty, value);
        }

        public BaseListBox()
        {
            this.WhenAnyValue(x => x.Items)
                .CombineLatest(this.WhenAnyValue(x => x.ScrollIndex), (items, index) => items != null && items.Count > index && index >= 0)
                .Subscribe(canScroll =>
                {
                    if (canScroll)
                    {
                        var scrollIndex = this.ScrollIndex;
                        var item = this.Items[scrollIndex];
                        Action scrollAction = () =>
                        {
                            this.Log().Debug("scroll into view for {0}", scrollIndex);
                            this.ScrollIntoView(item);
                        };
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, scrollAction);
                    }
                });

            this.Events().Loaded.Subscribe(e =>
            {
                var observeItemContGenerator = this.ObservableForProperty(x => x.ObserveItemContainerGenerator)
                    .Where(x => x.Value == true)
                    .Select(_ => Unit.Default);
                var itemContGeneratorStatusChanged = this.ItemContainerGenerator.Events().StatusChanged
                    .Throttle(TimeSpan.FromMilliseconds(150), RxApp.MainThreadScheduler)
                    .Select(_ => Unit.Default);
                Observable.Zip(observeItemContGenerator, itemContGeneratorStatusChanged).Subscribe(_ => this.FocusSelectedItem());
            });
        }

        private void FocusSelectedItem()
        {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            this.Log().Debug("ItemContainerGenerator status changed");
            if (this.Items.Count == 0) return;
            var index = this.SelectedIndex;
            if (index < 0) return;
            Action focusAction = () =>
            {
                this.ObserveItemContainerGenerator = false;
                if (this.Items.Count == 0) return;
                index = this.SelectedIndex;
                if (index < 0) return;
                this.Focus();
                this.ScrollIntoView(this.SelectedItem);
                var item = this.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
                if (item == null) return;
                item.Focus();
                this.Log().Debug("focus selected item {0} / {1}", index, item);
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, focusAction);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (!this.IsTextSearchEnabled && e.Key == Key.Space)
            {
                e.Handled = false;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
    }
}