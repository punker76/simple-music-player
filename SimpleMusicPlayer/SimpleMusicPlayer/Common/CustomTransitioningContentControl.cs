using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace SimpleMusicPlayer.Common
{
  public class CustomTransitioningContentControl : TransitioningContentControl
  {
    public static readonly DependencyProperty CustomVisualStatesProperty =
      DependencyProperty.Register("CustomVisualStates", typeof(List<VisualState>), typeof(CustomTransitioningContentControl), new PropertyMetadata(new List<VisualState>()));

    public List<VisualState> CustomVisualStates {
      get { return (List<VisualState>)this.GetValue(CustomVisualStatesProperty); }
      set { this.SetValue(CustomVisualStatesProperty, value); }
    }

    public override void OnApplyTemplate() {
      if (this.IsTransitioning) {
        this.AbortTransition();
      }

      var root = VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
      if (root != null) {
        var vsgroups = VisualStateManager.GetVisualStateGroups(root);
        if (vsgroups != null && this.CustomVisualStates != null && this.CustomVisualStates.Any()) {
          var group = vsgroups.OfType<VisualStateGroup>().FirstOrDefault();
          if (group != null) {
            foreach (var state in this.CustomVisualStates) {
              group.States.Add(state);
            }
          }
        }
      }

      base.OnApplyTemplate();
    }
  }
}