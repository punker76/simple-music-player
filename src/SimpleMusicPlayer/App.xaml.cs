using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
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
        public App()
        {
            TinyIoCContainer.Current.Register<AppHelper>().AsSingleton();
            TinyIoCContainer.Current.Register<CoverManager>().AsSingleton();
            TinyIoCContainer.Current.Register<PlayerSettings>().AsSingleton();
            TinyIoCContainer.Current.Register<PlayerEngine>().AsSingleton();
            TinyIoCContainer.Current.Register<IReactiveObject, MedialibViewModel>();
            TinyIoCContainer.Current.Register<IReactiveObject, MainViewModel>();

            var appHelper = TinyIoCContainer.Current.Resolve<AppHelper>();
            appHelper.ConfigureApp(this, Assembly.GetExecutingAssembly().GetName().Name);

            // Enable Multi-JIT startup
            var profileRoot = Path.Combine(appHelper.ApplicationPath, "ProfileOptimization");
            Directory.CreateDirectory(profileRoot);
            // Define the folder where to save the profile files
            ProfileOptimization.SetProfileRoot(profileRoot);
            // Start profiling and save it in Startup.profile
            ProfileOptimization.StartProfile("Startup.profile");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = TinyIoCContainer.Current.Resolve<MainWindow>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            TinyIoCContainer.Current.Resolve<AppHelper>().OnExitApp(this);
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
                    vm.PlayListsViewModel.CommandLineArgs = new ReactiveList<string>(args);
                }
            }
            return true;
        }
    }
}