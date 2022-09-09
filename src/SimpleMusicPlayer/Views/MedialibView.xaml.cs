using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for MedialibView.xaml
    /// </summary>
    public partial class MedialibView : MetroWindow, System.Windows.Forms.IWin32Window, IViewFor<MedialibViewModel>
    {
        public MedialibView(MedialibViewModel medialibViewModel)
        {
            this.ViewModel = medialibViewModel;
            this.ViewModel.WindowPlacementSettings = new CustomWindowPlacementSettings(this, this.ViewModel.PlayerSettings.Medialib);

            this.InitializeComponent();

            this.AllowDrop = true;

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.Events().SourceInitialized.Subscribe(e => this.FitIntoScreen());

            this.WhenActivated(d => this.WhenAnyValue(x => x.ViewModel)
                                        .Subscribe(vm => {
                                            this.Events().Closed.Select(x => Unit.Default).InvokeCommand(vm.FileSearchWorker.StopSearchCmd);
                                            this.Events().PreviewDragEnter.Merge(this.Events().PreviewDragOver).Subscribe(vm.OnDragOverAction);
                                            this.Events().PreviewDrop.SelectMany(x => vm.OnDropAction(x).ToObservable()).Subscribe();
                                        }));
        }

        public override IWindowPlacementSettings GetWindowPlacementSettings()
        {
            return this.ViewModel.WindowPlacementSettings;
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

        public MedialibViewModel ViewModel
        {
            get => (MedialibViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MedialibViewModel), typeof(MedialibView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MedialibViewModel)value;
        }
    }
}