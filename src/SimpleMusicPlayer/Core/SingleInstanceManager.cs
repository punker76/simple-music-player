#if !NETCOREAPP

using System;
using System.ServiceModel;
using System.Windows;

namespace SimpleMusicPlayer.Core
{
    /// <summary> The WCF interface for passing the startup parameters </summary>
    [ServiceContract]
    public interface ISingleInstance
    {
        /// <summary>
        /// Notifies the first instance that another instance of the application attempted to start.
        /// </summary>
        /// <param name="args">The other instance's command-line arguments.</param>
        [OperationContract]
        void PassStartupArgs(string[] args);
    }

    /// <summary>
    /// Event declaration for the startup of a second instance
    /// </summary>
    public class SecondInstanceStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The event method declaration for the startup of a second instance
        /// </summary>
        /// <param name="args">The other instance's command-line arguments.</param>
        public SecondInstanceStartedEventArgs(string[] args)
        {
            Args = args;
        }

        /// <summary>
        /// Property containing the second instance's command-line arguments
        /// </summary>
        public string[] Args { get; set; }
    }

    /// <summary>
    /// A class to allow for a single-instance of an application.
    /// </summary>
    public class SingleInstance : ISingleInstance
    {
        /// <summary>
        /// Is raised when another instance attempts to start up.
        /// </summary>
        public static event EventHandler<SecondInstanceStartedEventArgs> OnSecondInstanceStarted;

        private static ServiceHost NamedPipeHost = null;

        /// <summary>
        /// Interface: Notifies the first instance that another instance of the application attempted to start.
        /// </summary>
        /// <param name="args">The other instance's command-line arguments.</param>
        public void PassStartupArgs(string[] args)
        {
            //check if an event is registered for when a second instance is started
            OnSecondInstanceStarted?.Invoke(this, new SecondInstanceStartedEventArgs(args));
        }

        /// <summary>
        /// Checks to see if this instance is the first instance of this application on the local machine.  If it is not, this method will
        /// send the first instance this instance's command-line arguments.
        /// </summary>
        /// <param name="uid">The application's unique identifier.</param>
        /// <param name="activatewindow">Should the main window become active on a second instance launch</param>
        /// <returns>True if this instance is the first instance.</returns>
        public static bool IsFirstInstance(string uid, bool activatewindow)
        {
            //attempt to open the service, should succeed if this is the first instance
            if (OpenServiceHost(uid))
            {
                if (activatewindow) OnSecondInstanceStarted += ActivateMainWindow;
                return true;
            }

            //notify the first instance and pass the command-line args
            NotifyFirstInstance(uid);

            //ok to shutdown second instance
            Application.Current.Shutdown();
            return false;
        }

        /// <summary>
        /// Attempts to create the named pipe service.
        /// </summary>
        /// <param name="uid">The application's unique identifier.</param>
        /// <returns>True if the service was published successfully.</returns>
        private static bool OpenServiceHost(string uid)
        {
            try
            {
                //hook the application exit event to avoid race condition when messages flow while the application is disposing of the channel during shutdown
                Application.Current.Exit += new ExitEventHandler(OnAppExit);

                //setup the WCF service using a NamedPipe
                NamedPipeHost = new ServiceHost(typeof(SingleInstance), new Uri("net.pipe://localhost"));
                NamedPipeHost.AddServiceEndpoint(typeof(ISingleInstance), new NetNamedPipeBinding(), uid);

                //for any unhandled exception we need to ensure NamedPipeHost is disposed
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

                //if the service is already open (i.e. another instance is already running) this will cause an exception
                NamedPipeHost.Open();

                //success and we are first instance
                return true;
            }
            catch (AddressAlreadyInUseException)
            {
                //failed to open the service so must be a second instance
                NamedPipeHost.Abort();
                NamedPipeHost = null;
                return false;
            }
            catch (CommunicationObjectFaultedException)
            {
                NamedPipeHost.Abort();
                NamedPipeHost = null;
                return false;
            }
        }

        /// <summary>
        /// Ensures that the named pipe service host is closed on the application exit
        /// </summary>
        private static void OnAppExit(object sender, EventArgs e)
        {
            if (NamedPipeHost != null)
            {
                NamedPipeHost.Close();
                NamedPipeHost = null;
            }
        }

        /// <summary>
        /// ensure host is disposed of if there is an unhandled exception
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (NamedPipeHost != null)
            {
                if (NamedPipeHost.State == CommunicationState.Faulted)
                    NamedPipeHost.Abort();
                else
                    NamedPipeHost.Close();
                NamedPipeHost = null;
            }
        }

        /// <summary>
        /// Notifies the main instance that this instance is attempting to start up.
        /// </summary>
        /// <param name="uid">The application's unique identifier.</param>
        private static void NotifyFirstInstance(string uid)
        {
            //create channel with first instance interface
            using (ChannelFactory<ISingleInstance> factory = new ChannelFactory<ISingleInstance>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/" + uid)))
            {
                ISingleInstance singleInstanceInterface = factory.CreateChannel();
                //pass the command-line args to the first interface
                singleInstanceInterface.PassStartupArgs(Environment.GetCommandLineArgs());
            }
        }

        /// <summary>
        /// Activate the first instance's main window
        /// </summary>
        private static void ActivateMainWindow(object sender, SecondInstanceStartedEventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var window = Application.Current.MainWindow;
                if (window is null)
                {
                    return;
                }

                if (window.WindowState == WindowState.Minimized)
                {
                    WindowExtensions.Unminimize(window);
                }
                else
                {
                    WindowExtensions.ShowAndActivate(window);
                }
            });
        }
    }
}

#endif