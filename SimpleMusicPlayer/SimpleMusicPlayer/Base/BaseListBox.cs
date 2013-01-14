using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleMusicPlayer.Base
{
  public class BaseListBox : ListBox
  {
    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
      if (!this.IsTextSearchEnabled && e.Key == Key.Space) {
        e.Handled = false;
      } else {
        base.OnKeyDown(e);
      }
    }
  }
}