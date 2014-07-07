using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SimpleMusicPlayer.Base
{
  public class SelectorScrollViewerAttachedBehavior
  {
    public static readonly DependencyProperty ScrollingLinesProperty
      = DependencyProperty.RegisterAttached("ScrollingLines",
                                            typeof(int),
                                            typeof(SelectorScrollViewerAttachedBehavior),
                                            new UIPropertyMetadata(3, OnScrollingLinesPropertyChangedCallback));

    public static readonly DependencyProperty ScrollViewerProperty
      = DependencyProperty.RegisterAttached("ScrollViewer",
                                            typeof(ScrollViewer),
                                            typeof(SelectorScrollViewerAttachedBehavior),
                                            new UIPropertyMetadata(null));

    public static int GetScrollingLines(DependencyObject source)
    {
      return (int)source.GetValue(ScrollingLinesProperty);
    }

    public static void SetScrollingLines(DependencyObject source, int value)
    {
      source.SetValue(ScrollingLinesProperty, value);
    }

    public static ScrollViewer GetScrollViewer(DependencyObject source)
    {
      return (ScrollViewer)source.GetValue(ScrollViewerProperty);
    }

    public static void SetScrollViewer(DependencyObject source, ScrollViewer value)
    {
      source.SetValue(ScrollViewerProperty, value);
    }

    private static void OnScrollingLinesPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var lb = dependencyObject as Selector;
      if (lb != null && e.NewValue != e.OldValue && e.NewValue is int) {
        lb.Loaded -= OnSelectorLoaded;
        lb.Loaded += OnSelectorLoaded;
      }
    }

    private static void OnSelectorLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      var lb = sender as Selector;
      if (lb != null) {
        // get or store scrollviewer
        SetScrollViewer(lb, lb.GetDescendantByType(typeof(ScrollViewer)) as ScrollViewer);
        lb.PreviewMouseWheel -= OnSelectorPreviewMouseWheel;
        lb.PreviewMouseWheel += OnSelectorPreviewMouseWheel;
      }
    }

    private static void OnSelectorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      var lb = sender as Selector;
      if (lb != null) {
        // get or store scrollviewer
        var lbScrollViewer = GetScrollViewer(lb);
        if (lbScrollViewer != null) {
          var scrollingLines = GetScrollingLines(lb);
          if (e.Delta < 0) {
            for (var i = 0; i < scrollingLines; i++) {
              lbScrollViewer.LineDown();
            }
          } else {
            for (var i = 0; i < scrollingLines; i++) {
              lbScrollViewer.LineUp();
            }
          }
          e.Handled = true;
        }
      }
    }
  }
}