using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                    vm.PlayListsViewModel.CommandLineArgs = new ReactiveList<string>(args);
                }
            }
            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var container = TinyIoCContainer.Current;

            container.Register<AppHelper>().AsSingleton();
            container.Register<PlayerSettings>().AsSingleton();
            container.Register<PlayerEngine>().AsSingleton();
            container.Register<IReactiveObject, MainViewModel>();

            container.Resolve<AppHelper>().ConfigureApp(this, Assembly.GetExecutingAssembly().GetName().Name);

            MainWindow = container.Resolve<MainWindow>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            var container = TinyIoCContainer.Current;
            container.Resolve<AppHelper>().OnExitApp(this);
        }
    }
}