using MahApps.Metro.Controls;
using SimpleMusicPlayer.ViewModels;

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
    }
  }
}