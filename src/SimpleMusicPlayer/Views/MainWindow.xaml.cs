using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.ViewModels;
using Splat;

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
            this.ViewModel.WindowPlacementSettings = new CustomWindowPlacementSettings(this, this.ViewModel.PlayerSettings.MainWindow);

            this.InitializeComponent();

            //this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);
            this.Title = string.Format("{0}", this.Title);

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.Events().SourceInitialized.Subscribe(e => this.FitIntoScreen());

            // load playlist and command line stuff
            this.Events().Loaded.Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler).Select(x => Unit.Default).InvokeCommand(this.ViewModel, x => x.PlayListsViewModel.StartUpCommand);

            this.WhenActivated(d => this.WhenAnyValue(x => x.ViewModel)
                                        .Subscribe(vm => {
                                            IObservable<KeyEventArgs> previewKeyDown = this.Events().PreviewKeyDown;
                                            // handle main view keys
                                            previewKeyDown.Subscribe(vm.HandlePreviewKeyDown);
                                            // handle playlist keys
                                            previewKeyDown.Where(x => x.Key == Key.Enter).DistinctUntilChanged(x => x.IsToggled).InvokeCommand(vm.PlayListsViewModel.PlayCommand);
                                            previewKeyDown.Where(x => x.Key == Key.Delete).InvokeCommand(vm.PlayListsViewModel.DeleteCommand);
                                            previewKeyDown.Where(x => x.Key == Key.L).DistinctUntilChanged(x => x.IsToggled).Select(x => Unit.Default).InvokeCommand(vm.PlayControlInfoViewModel.PlayControlViewModel.ShowMediaLibraryCommand);

                                            previewKeyDown.Where(x => x.Key == Key.R).DistinctUntilChanged(x => x.IsToggled).Select(x => Unit.Default).InvokeCommand(vm.PlayControlInfoViewModel.PlayControlViewModel.RepeatCommand);
                                            previewKeyDown.Where(x => x.Key == Key.S).DistinctUntilChanged(x => x.IsToggled).Select(x => Unit.Default).InvokeCommand(vm.PlayControlInfoViewModel.PlayControlViewModel.ShuffleCommand);
                                            previewKeyDown.Where(x => x.Key == Key.M).DistinctUntilChanged(x => x.IsToggled).Select(x => Unit.Default).InvokeCommand(vm.PlayControlInfoViewModel.PlayControlViewModel.MuteCommand);
                                            previewKeyDown.Where(x => x.Key == Key.E).DistinctUntilChanged(x => x.IsToggled).Select(x => Unit.Default).InvokeCommand(vm.PlayControlInfoViewModel.PlayControlViewModel.ShowEqualizerCommand);

                                            var window = Window.GetWindow(this);
                                            if (window != null)
                                            {
                                                SizeTriggerHelper.Instance.CalcSize(window.ActualWidth, window.ActualHeight);
                                                window.Events()
                                                    .SizeChanged
                                                    .Throttle(TimeSpan.FromMilliseconds(15), RxApp.MainThreadScheduler)
                                                    .Subscribe(e => SizeTriggerHelper.Instance.CalcSize(e.NewSize.Width, e.NewSize.Height));
                                            }

                                            this.Events().Closed.Select(x => Unit.Default).InvokeCommand(vm.PlayListsViewModel.FileSearchWorker.StopSearchCmd);
                                            this.Events().Closed.Subscribe(x => vm.ShutDown());

                                            this.DpiChanged += MainWindow_DpiChanged;
                                        }));
        }

        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            this.ViewModel.Log().Debug($"DpiChanged: old={e.OldDpi.PixelsPerDip} new={e.NewDpi.PixelsPerDip}");
        }

        public override IWindowPlacementSettings GetWindowPlacementSettings()
        {
            return this.ViewModel.WindowPlacementSettings;
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
    }
}