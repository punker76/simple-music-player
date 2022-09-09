using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using ReactiveUI;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public class BaseListView : ListView, IEnableLogger
    {
        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register(
            nameof(ScrollViewer), typeof(ScrollViewer), typeof(BaseListView), new PropertyMetadata(default(ScrollViewer)));

        public ScrollViewer ScrollViewer
        {
            get => (ScrollViewer)GetValue(ScrollViewerProperty);
            set => SetValue(ScrollViewerProperty, value);
        }

        public static readonly DependencyProperty VerticalScrollBarWidthProperty = DependencyProperty.Register(
            nameof(VerticalScrollBarWidth), typeof(double), typeof(BaseListView), new PropertyMetadata(0d));

        public double VerticalScrollBarWidth
        {
            get => (double)GetValue(VerticalScrollBarWidthProperty);
            set => SetValue(VerticalScrollBarWidthProperty, value);
        }

        public static readonly DependencyProperty ObserveItemContainerGeneratorProperty
            = DependencyProperty.Register(nameof(ObserveItemContainerGenerator),
                typeof(bool),
                typeof(BaseListView),
                new PropertyMetadata(default(bool)));

        public bool ObserveItemContainerGenerator
        {
            get => (bool)this.GetValue(ObserveItemContainerGeneratorProperty);
            set => this.SetValue(ObserveItemContainerGeneratorProperty, value);
        }

        public static readonly DependencyProperty ScrollIndexProperty
            = DependencyProperty.Register(nameof(ScrollIndex),
                typeof(int),
                typeof(BaseListView),
                new PropertyMetadata(-1));

        public int ScrollIndex
        {
            get => (int)this.GetValue(ScrollIndexProperty);
            set => this.SetValue(ScrollIndexProperty, value);
        }

        public BaseListView()
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
                var item = this.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
                if (item == null) return;
                item.Focus();
                this.Log().Debug("focus selected item {0} / {1}", index, item);
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, focusAction);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.GetTheScrollViewer();
        }

        private void GetTheScrollViewer()
        {
            if (this.ScrollViewer == null)
            {
                this.ScrollViewer = this.GetDescendantByType(typeof(ScrollViewer)) as ScrollViewer;
                if (this.ScrollViewer != null)
                {
                    this.ScrollViewer.Loaded += ScrollViewer_Loaded;
                }
            }
        }

        void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var scrollBar = scrollViewer.FindChild<ScrollBar>("PART_VerticalScrollBar");
            if (scrollBar != null)
            {
                VerticalScrollBarWidth = scrollBar.Width;
            }
        }
    }

    public class AutoGeneratedListView : BaseListView
    {
        public Type DataType { get; set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "ItemsSource"
                && e.OldValue != e.NewValue
                && e.NewValue != null
                && this.DataType != null)
            {
                CreateColumns(this);
            }
        }

        private static void CreateColumns(AutoGeneratedListView lv)
        {
            var gridView = new GridView { AllowsColumnReorder = true };

            // get only declared properties from DataType
            var properties = lv.DataType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var pi in properties)
            {
                var browsableAttribute = pi.GetCustomAttributes(true).FirstOrDefault(a => a is BrowsableAttribute) as BrowsableAttribute;
                if (browsableAttribute != null && !browsableAttribute.Browsable)
                {
                    continue;
                }

                var binding = new Binding { Path = new PropertyPath(pi.Name), Mode = BindingMode.OneWay };
                var gridViewColumn = new GridViewColumn() { Header = pi.Name, DisplayMemberBinding = binding };
                gridView.Columns.Add(gridViewColumn);
            }

            lv.View = gridView;
        }

        //var gridViewColumn = new GridViewColumn() { Header = pi.Name, CellTemplate = GetCellTemplate(binding) };
        private static DataTemplate GetCellTemplate(Binding binding)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            //factory.SetValue(RenderOptions.ClearTypeHintProperty, ClearTypeHint.Enabled);
            //factory.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            //factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            factory.SetBinding(TextBlock.TextProperty, binding);
            template.VisualTree = factory;

            return template;
        }
    }
}