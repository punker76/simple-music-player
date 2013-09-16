using System;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for MedialibView.xaml
  /// </summary>
  public partial class MedialibView : MetroWindow, System.Windows.Forms.IWin32Window
  {
    public MedialibView(MedialibViewModel medialibViewModel) {
      this.DataContext = medialibViewModel;

      this.InitializeComponent();

      this.AllowDrop = true;

      this.SourceInitialized += (sender, e) => this.FitIntoScreen();

      this.Closed += (sender, e) => {
                       var viewModel = ((MedialibViewModel)this.DataContext);
                       if (viewModel.FileSearchWorker.CanStopSearch()) {
                         viewModel.FileSearchWorker.StopSearch();
                       }
                     };
    }

    // only for ShowDialog from FolderBrowserDialog
    public IntPtr Handle {
      get {
        var intPtr = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
        return intPtr;
      }
    }
  }
}