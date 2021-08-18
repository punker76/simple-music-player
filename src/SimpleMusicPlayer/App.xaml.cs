using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Windows;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Player;
using SimpleMusicPlayer.ViewModels;
using SimpleMusicPlayer.Views;
using TinyIoC;
#if NETCOREAPP
using SingleInstanceCore;
#endif

namespace SimpleMusicPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
#if NETCOREAPP
    public partial class App : Application, ISingleInstance
#else
    public partial class App : Application
#endif
    {
        public App()
        {
            // check if we are the first instance
#if NETCOREAPP
            var isFirstInstance = this.InitializeAsFirstInstance("18980929-1342-4467-bc3d-37b0d13fa938");
#else
            var isFirstInstance = SingleInstance.IsFirstInstance("18980929-1342-4467-bc3d-37b0d13fa938", true);
#endif

            if (!isFirstInstance)
            {
                //If it's not the first instance, arguments are automatically passed to the first instance
                //OnInstanceInvoked will be raised on the first instance
                //You may shut down the current instance
                Current.Shutdown();
            }
            else
            {
#if !NETCOREAPP
                //we are, register our event handler for receiving the new arguments
                SingleInstance.OnSecondInstanceStarted += NewStartupArgs;
#endif

                //place additional startup code here
                // SplashScreen splashScreen = new SplashScreen("SplashScreen.jpg");
                // splashScreen.Show(true);

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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = TinyIoCContainer.Current.Resolve<MainWindow>();
            MainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TinyIoCContainer.Current.Resolve<AppHelper>().OnExitApp(this);

            base.OnExit(e);
        }

#if NETCOREAPP

        public void OnInstanceInvoked(string[] args)
        {
            //What to do with the args another instance has sent
            ProcessCommandLineArgs(args);
        }

#else

        private void NewStartupArgs(object sender, SecondInstanceStartedEventArgs e)
        {
            //handle new startup arguments and/or do anything else for second instance launch
            ProcessCommandLineArgs(e.Args);
        }

#endif

        private void ProcessCommandLineArgs(IEnumerable<string> args)
        {
            Current?.Dispatcher?.Invoke(() =>
            {
                var window = Current.MainWindow;
                if (window?.DataContext is MainViewModel vm)
                {
                    vm.PlayListsViewModel.CommandLineArgs = new ReactiveList<string>(args);
                }
            });
        }
    }
}