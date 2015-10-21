using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IViewFor<MainViewModel>
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            this.ViewModel = mainViewModel;

            this.InitializeComponent();

            //this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);
            this.Title = string.Format("{0}", this.Title);

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.Events().SourceInitialized.Subscribe(e => this.FitIntoScreen());

            // load playlist and command line stuff
            this.Events().Loaded.Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler).InvokeCommand(this.ViewModel.PlayListsViewModel.StartUpCommand);

            this.WhenActivated(d => this.WhenAnyValue(x => x.ViewModel)
                                        .Subscribe(vm => {
                                            var previewKeyDown = this.Events().PreviewKeyDown;
                                            // handle main view keys
                                            previewKeyDown.Subscribe(vm.HandlePreviewKeyDown);
                                            // handle playlist keys
                                            previewKeyDown.Where(x => x.Key == Key.Enter).InvokeCommand(vm.PlayListsViewModel.PlayCommand);
                                            previewKeyDown.Where(x => x.Key == Key.Delete).InvokeCommand(vm.PlayListsViewModel.DeleteCommand);

                                            var window = Window.GetWindow(this);
                                            if (window != null)
                                            {
                                                vm.PlayListsViewModel.CalcPlayListItemTemplateByActualWidth(window.ActualWidth, window.ActualHeight);
                                                window.Events().SizeChanged.Throttle(TimeSpan.FromMilliseconds(15), RxApp.MainThreadScheduler).Subscribe(e => vm.PlayListsViewModel.CalcPlayListItemTemplateByActualWidth(e.NewSize.Width, e.NewSize.Height));
                                            }

                                            this.Events().Closed.InvokeCommand(vm.PlayListsViewModel.FileSearchWorker.StopSearchCmd);
                                            this.Events().Closed.Subscribe(x => vm.ShutDown());
                                        }));
        }

        public override IWindowPlacementSettings GetWindowPlacementSettings()
        {
            return this.ViewModel.CustomWindowPlacementSettings;
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }
    }
}