using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    public MainWindow() {
      var vm = new MainWindowViewModel(this.Dispatcher);
      this.DataContext = vm;

      this.InitializeComponent();

      this.PreviewKeyDown += (sender, e) => (this.DataContext as IKeyHandler).HandlePreviewKeyDown(sender, e);

      this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);

      this.SourceInitialized += (sender, e) => this.FitIntoScreen();

      this.Closed += (sender, e) => {
                       foreach (var w in Application.Current.Windows.OfType<Window>()) {
                         w.Close();
                       }
                       var mainWindowViewModel = ((MainWindowViewModel)this.DataContext);
                       if (mainWindowViewModel.PlaylistsViewModel.FileSearchWorker.CanStopSearch()) {
                         mainWindowViewModel.PlaylistsViewModel.FileSearchWorker.StopSearch();
                       }
                       mainWindowViewModel.SaveSettings();
                       mainWindowViewModel.PlaylistsViewModel.SavePlayList();
                       PlayerEngine.Instance.CleanUp();
                     };

      vm.ProcessCommandLineArgs(Environment.GetCommandLineArgs().ToList());
    }
  }
}