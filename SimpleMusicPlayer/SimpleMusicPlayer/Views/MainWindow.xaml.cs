using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Player;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IViewFor<MainViewModel>
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.WhenActivated(d => this.WhenAnyValue(x => x.ViewModel)
                                        .Subscribe(vm => {
                                            this.Events().SourceInitialized.Subscribe(e => this.FitIntoScreen());

                                            this.Events().PreviewKeyDown.Subscribe(vm.HandlePreviewKeyDown);

                                            this.Events().Closed.InvokeCommand(vm.PlayListsViewModel.FileSearchWorker.StopSearchCmd);

                                            this.Events().Closed.Subscribe(async e => {
                                                foreach (var w in Application.Current.Windows.OfType<Window>())
                                                {
                                                    w.Close();
                                                }
                                                vm.SaveSettings();
                                                await vm.PlayListsViewModel.SavePlayListAsync();
                                                PlayerEngine.Instance.CleanUp();
                                            });
                                        }));

            this.ViewModel = new MainViewModel(this.Dispatcher);
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