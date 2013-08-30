using MahApps.Metro.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for MedialibView.xaml
  /// </summary>
  public partial class MedialibView : MetroWindow
  {
    public MedialibView(MedialibViewModel medialibViewModel) {
      this.DataContext = medialibViewModel;
      this.InitializeComponent();
      this.AllowDrop = true;
      this.SourceInitialized += (sender, e) => this.FitIntoScreen();
    }
  }
}