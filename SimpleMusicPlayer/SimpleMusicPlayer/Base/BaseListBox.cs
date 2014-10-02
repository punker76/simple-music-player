using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ReactiveUI;

namespace SimpleMusicPlayer.Base
{
  public class BaseListBox : ListBox
  {
    public static readonly DependencyProperty ObserveItemContainerGeneratorProperty = DependencyProperty.Register(
      "ObserveItemContainerGenerator", typeof(bool), typeof(BaseListBox), new PropertyMetadata(default(bool)));

    public bool ObserveItemContainerGenerator
    {
      get { return (bool)GetValue(ObserveItemContainerGeneratorProperty); }
      set { SetValue(ObserveItemContainerGeneratorProperty, value); }
    }

    public BaseListBox()
    {
      this.Loaded += (s, e) => Observable.Zip(
        this.ObservableForProperty(x => x.ObserveItemContainerGenerator).Where(x => x.Value == true).Select(_ => Unit.Default),
        this.ItemContainerGenerator.Events().StatusChanged.Throttle(TimeSpan.FromMilliseconds(150), RxApp.MainThreadScheduler).Select(_ => Unit.Default)
        ).Subscribe(_ => FocusSelectedItem());
    }

    private void FocusSelectedItem()
    {
      if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
      this.ObserveItemContainerGenerator = false;
      Debug.WriteLine(">>>>>>>status changed");
      if (this.Items.Count == 0) return;
      var index = this.SelectedIndex;
      if (index < 0) return;
      Action focusAction = () => {
        if (this.Items.Count == 0) return;
        index = this.SelectedIndex;
        if (index < 0) return;
        this.Focus();
        this.ScrollIntoView(this.SelectedItem);
        var item = this.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        if (item == null) return;
        item.Focus();
        Debug.WriteLine(">>>>>>>focus selected item");
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