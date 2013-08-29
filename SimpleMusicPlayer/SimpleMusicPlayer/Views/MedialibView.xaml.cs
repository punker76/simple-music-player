using MahApps.Metro.Controls;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for MedialibView.xaml
  /// </summary>
  public partial class MedialibView : MetroWindow
  {
    public MedialibView() {
      this.InitializeComponent();
      this.AllowDrop = true;
      this.SourceInitialized += (sender, e) => this.FitIntoScreen();
    }
  }
}