using System.Reflection;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.ViewModels;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer
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
                       ((MainWindowViewModel)this.DataContext).SaveSettings();
                       PlayerEngine.Instance.CleanUp();
                     };
    }
  }
}