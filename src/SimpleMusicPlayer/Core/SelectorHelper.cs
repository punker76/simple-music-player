using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SimpleMusicPlayer.Core
{
    public static class SelectorHelper
    {
        public static readonly DependencyProperty ScrollingLinesProperty =
          DependencyProperty.RegisterAttached("ScrollingLines",
                                              typeof(int),
                                              typeof(SelectorHelper),
                                              new UIPropertyMetadata(3,
                                                                     OnScrollingLinesPropertyChangedCallback,
                                                                     (o, value) => (int)value <= 0 ? 1 : value));

        /// <summary>
        /// Gets or Sets the value how much lines (items) should be scrolled.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(ListBox))]
        public static int GetScrollingLines(Selector source)
        {
            return (int)source.GetValue(ScrollingLinesProperty);
        }

        public static void SetScrollingLines(Selector source, int value)
        {
            source.SetValue(ScrollingLinesProperty, value);
        }

        private static readonly DependencyProperty ScrollViewerProperty =
          DependencyProperty.RegisterAttached("ScrollViewer",
                                              typeof(ScrollViewer),
                                              typeof(SelectorHelper),
                                              new UIPropertyMetadata(null));

        private static ScrollViewer GetScrollViewer(DependencyObject source)
        {
            return (ScrollViewer)source.GetValue(ScrollViewerProperty);
        }

        private static void SetScrollViewer(DependencyObject source, ScrollViewer value)
        {
            source.SetValue(ScrollViewerProperty, value);
        }

        private static void OnScrollingLinesPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var selector = (Selector)dependencyObject;
            if (e.NewValue != e.OldValue && e.NewValue != null)
            {
                selector.Loaded -= OnSelectorLoaded;
                selector.Loaded += OnSelectorLoaded;
            }
        }

        private static void OnSelectorLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var selector = (Selector)sender;
            // get the scrollviewer
            SetScrollViewer(selector,
                            selector.GetDescendantByType(typeof(ScrollViewer)) as ScrollViewer);
            selector.PreviewMouseWheel -= OnSelectorPreviewMouseWheel;
            selector.PreviewMouseWheel += OnSelectorPreviewMouseWheel;
        }

        private static void OnSelectorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0)
            {
                // nothing to do
                return;
            }

            var selector = (Selector)sender;
            // get or stored scrollviewer
            var lbScrollViewer = GetScrollViewer(selector);
            if (lbScrollViewer != null)
            {
                var scrollingLines = GetScrollingLines(selector);
                for (var i = 0; i < scrollingLines; i++)
                {
                    if (e.Delta < 0)
                    {
                        lbScrollViewer.LineDown();
                    }
                    else
                    {
                        lbScrollViewer.LineUp();
                    }
                }
                e.Handled = true;
            }
        }
    }
}