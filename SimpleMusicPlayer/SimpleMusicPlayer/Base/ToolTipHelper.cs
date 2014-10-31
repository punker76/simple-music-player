using System;
using System.Windows;
using System.Windows.Controls;

namespace SimpleMusicPlayer.Base
{
  public static class ToolTipHelper
  {
    public static readonly DependencyProperty AutoMoveProperty =
            DependencyProperty.RegisterAttached("AutoMove",
            typeof(bool),
            typeof(ToolTipHelper),
            new FrameworkPropertyMetadata(false, AutoMoveCasePropertyChangedCallback));

    /// <summary>
    /// ToolTip follows the mouse movement.
    /// When set to <c>true</c>, the tool tip follows the mouse movement.
    /// </summary>
    [AttachedPropertyBrowsableForType(typeof(ToolTip))]
    public static bool GetAutoMove(ToolTip element)
    {
      return (bool)element.GetValue(AutoMoveProperty);
    }

    public static void SetAutoMove(ToolTip element, bool value)
    {
      element.SetValue(AutoMoveProperty, value);
    }

    private static void AutoMoveCasePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      var toolTip = (ToolTip)dependencyObject;
      if (eventArgs.OldValue != eventArgs.NewValue && eventArgs.NewValue != null)
      {
        var autoMove = (bool)eventArgs.NewValue;
        if (autoMove)
        {
          toolTip.Loaded += ToolTipLoaded;
          toolTip.Unloaded += ToolTipUnloaded;
        }
        else
        {
          toolTip.Loaded -= ToolTipLoaded;
          toolTip.Unloaded -= ToolTipUnloaded;
        }
      }
    }

    private static void ToolTipLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      var toolTip = (ToolTip)sender;
      var target = toolTip.PlacementTarget as FrameworkElement;
      if (target != null)
      {
        target.MouseMove += ToolTipTargetPreviewMouseMove;
      }
    }

    static void ToolTipUnloaded(object sender, RoutedEventArgs e)
    {
      var toolTip = (ToolTip)sender;
      var target = toolTip.PlacementTarget as FrameworkElement;
      if (target != null)
      {
        target.MouseMove -= ToolTipTargetPreviewMouseMove;
      }
    }

    static void ToolTipTargetPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      var target = sender as FrameworkElement;
      var toolTip = (target != null ? target.ToolTip : null) as ToolTip;
      if (toolTip != null)
      {
        toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
        toolTip.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 16;
        toolTip.VerticalOffset = e.GetPosition((IInputElement)sender).Y + 16;
      }
    }
  }
}
