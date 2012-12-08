using System.Windows;
using System.Windows.Controls;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for PlaylistsView.xaml
  /// </summary>
  public partial class PlaylistsView : UserControl
  {
    public PlaylistsView() {
      this.InitializeComponent();

      this.AllowDrop = true;

      this.DataContextChanged += (s, ea) => {
        var vm = ea.NewValue as PlaylistsViewModel;
        if (vm != null) {
          // Override this to allow drop functionality.
          this.PreviewDragOver += (sender, e) => e.Handled = true;
          this.PreviewDragEnter += (sender, e) => e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
          this.PreviewDrop += (sender, e) => {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
              // Get data object
              var dataObject = e.Data as DataObject;
              if (dataObject != null && dataObject.ContainsFileDropList()) {
                vm.HandleDropActionAsync(dataObject.GetFileDropList());
              }
            }
          };
        }
      };
    }
  }
}