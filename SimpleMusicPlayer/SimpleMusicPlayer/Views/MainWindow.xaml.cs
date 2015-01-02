using System;
using System.Reflection;
using System.Windows;
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

            this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.Events().SourceInitialized.Subscribe(e => this.FitIntoScreen());

            this.Events().PreviewKeyDown.Subscribe(this.ViewModel.HandlePreviewKeyDown);

            this.Events().Closed.InvokeCommand(this.ViewModel.PlayListsViewModel.FileSearchWorker.StopSearchCmd);
            this.Events().Closed.Subscribe(x=>this.ViewModel.ShutDown());
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