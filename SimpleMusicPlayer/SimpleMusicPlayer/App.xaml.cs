using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Shell;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Player;
using SimpleMusicPlayer.ViewModels;
using SimpleMusicPlayer.Views;
using TinyIoC;

namespace SimpleMusicPlayer
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
                WindowExtensions.Unminimize(this.MainWindow);
            }
            else
            {
                WindowExtensions.ShowAndActivate(this.MainWindow);
            }
            return this.ProcessCommandLineArgs(this.MainWindow as SimpleMusicPlayer.Views.MainWindow, args);
        }

        private bool ProcessCommandLineArgs(SimpleMusicPlayer.Views.MainWindow window, IEnumerable<string> args)
        {
            if (window != null)
            {
                var vm = window.DataContext as SimpleMusicPlayer.ViewModels.MainViewModel;
                if (vm != null)
                {
                    vm.PlayListsViewModel.HandleCommandLineArgsAsync(args.ToList());
                }
            }
            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var container = TinyIoCContainer.Current;

            container.Register<PlayerSettings>().AsSingleton();
            container.Register<IReactiveObject, MainViewModel>();

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}