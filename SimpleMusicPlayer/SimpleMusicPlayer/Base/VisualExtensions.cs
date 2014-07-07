using System;
using System.Windows;
using System.Windows.Media;

namespace SimpleMusicPlayer.Base
{
  public static class VisualExtensions
  {
    public static Visual GetDescendantByType(this Visual element, Type type)
    {
      if (element == null) {
        return null;
      }
      if (element.GetType() == type) {
        return element;
      }
      Visual foundElement = null;
      if (element is FrameworkElement) {
        (element as FrameworkElement).ApplyTemplate();
      }
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
        var visual = VisualTreeHelper.GetChild(element, i) as Visual;
        foundElement = GetDescendantByType(visual, type);
        if (foundElement != null) {
          break;
        }
      }
      return foundElement;
    }
  }
}