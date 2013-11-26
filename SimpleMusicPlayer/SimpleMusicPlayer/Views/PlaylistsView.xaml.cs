using System.Windows;
using System.Windows.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for PlaylistsView.xaml
  /// </summary>
  public partial class PlaylistsView : UserControl
  {
    public PlaylistsView()
    {
      this.InitializeComponent();

      this.PreviewKeyDown += (sender, e) => (this.DataContext as IKeyHandler).HandlePreviewKeyDown(sender, e);

      this.DataContextChanged += this.PlaylistsView_DataContextChanged;
    }

    private void PlaylistsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
      var vm = this.DataContext as PlaylistsViewModel;
      if (vm != null) {
        var window = Window.GetWindow(this);
        if (window != null) {
          window.SizeChanged += (o, args) => vm.CalcPlayListItemTemplateByActualWidth(window.ActualWidth);
        }
      }
    }
  }
}