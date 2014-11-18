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
        public MedialibView(MedialibViewModel medialibViewModel)
        {
            this.DataContext = medialibViewModel;

            this.InitializeComponent();

            this.AllowDrop = true;

            this.SourceInitialized += (sender, e) => this.FitIntoScreen();

            this.Closed += (sender, e) => {
                var viewModel = ((MedialibViewModel)this.DataContext);
                if (viewModel.FileSearchWorker.CanStopSearch())
                {
                    viewModel.FileSearchWorker.StopSearch();
                }
            };

            // Override this to allow drop functionality.
            DragEventHandler previewDragOver = (sender, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var viewModel = (MedialibViewModel)this.DataContext;
                    e.Effects = viewModel.FileSearchWorker.CanStartSearch() ? DragDropEffects.Copy : DragDropEffects.None;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            };
            this.PreviewDragOver += previewDragOver;
            this.PreviewDragEnter += previewDragOver;
            this.PreviewDrop += (sender, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var viewModel = (MedialibViewModel)this.DataContext;
                    // Get data object
                    var dataObject = e.Data as DataObject;
                    if (dataObject != null && dataObject.ContainsFileDropList())
                    {
                        viewModel.HandleDropAction(dataObject.GetFileDropList());
                    }
                }
            };
        }

        // only for ShowDialog from FolderBrowserDialog
        public IntPtr Handle
        {
            get
            {
                var intPtr = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
                return intPtr;
            }
        }
    }
}