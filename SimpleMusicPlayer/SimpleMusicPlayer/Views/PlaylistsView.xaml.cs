using System.Windows.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for PlaylistsView.xaml
  /// </summary>
  public partial class PlaylistsView : UserControl
  {
    public PlaylistsView() {
      this.InitializeComponent();
      
      this.PreviewKeyDown += (sender, e) => (this.DataContext as IKeyHandler).HandlePreviewKeyDown(sender, e);
    }
  }
}