using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Shell;

namespace SimpleMusicPlayerApplication
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ISingleInstanceApp
  {
    public void Init()
    {
      this.InitializeComponent();
    }

    public bool SignalExternalCommandLineArgs(IList<string> args)
    {
      if (this.MainWindow.WindowState == WindowState.Minimized)
      {
        SimpleMusicPlayer.Base.WindowExtensions.Unminimize(this.MainWindow);
      }
      else
      {
        SimpleMusicPlayer.Base.WindowExtensions.ShowAndActivate(this.MainWindow);
      }
      return ProcessCommandLineArgs(this.MainWindow as SimpleMusicPlayer.Views.MainWindow, args);
    }

    private bool ProcessCommandLineArgs(SimpleMusicPlayer.Views.MainWindow window, IEnumerable<string> args)
    {
      if (window != null) {
        var vm = window.DataContext as SimpleMusicPlayer.ViewModels.MainWindowViewModel;
        if (vm != null) {
          vm.PlayListsViewModel.HandleCommandLineArgsAsync(args.ToList());
        }
      }
      return true;
    }
  }
}