using System.Windows;
using System.Windows.Controls;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for MedialibView.xaml
  /// </summary>
  public partial class MedialibView : UserControl
  {
    public MedialibView() {
      this.InitializeComponent();

      this.AllowDrop = true;

      this.DataContextChanged += (s, ea) => {
        var vm = ea.NewValue as MedialibViewModel;
        if (vm != null) {
          // Override this to allow drop functionality.
          this.PreviewDragOver += (sender, e) => {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) && FileSearchWorker.Instance.CanStartSearch() ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
          };
          this.PreviewDrop += (sender, e) => {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
              // Get data object
              var dataObject = e.Data as DataObject;
              if (dataObject != null && dataObject.ContainsFileDropList()) {
                vm.HandleDropAction(dataObject.GetFileDropList());
              }
            }
          };
        }
      };
    }
  }
}