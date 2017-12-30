using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using Splat;
using TinyIoC;

namespace SimpleMusicPlayer.Core
{
    public sealed class AppHelper : IEnableLogger
    {
#if DEBUG
        private const int GCTimerPeriod = 10000;
#else
        private const int GCTimerPeriod = 30000;
#endif
        private Timer gcTimer;
        private static readonly object lockErrorDialog = 1;
        private static bool blockExceptionDialog;

        public void ConfigureApp(Application app, string appName)
        {
            this.ApplicationStartedTime = DateTime.UtcNow;
            this.ApplicationName = appName;

            // check where we can write
            if (CanCreateFile("."))
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                ApplicationPath = Path.GetDirectoryName(path);
            }
            else
            {
                ApplicationPath = ApplicationDataPath();
            }
            //this.NLogFileName = "${specialfolder:dir=logs:file=" + this.ApplicationName + ".${shortdate}.log:folder=" + this.ApplicationPath + "}";
            this.NLogFileName = Path.Combine(this.ApplicationPath, "logs", this.ApplicationName + ".${shortdate}.log").Replace("\\", "/");

            this.ConfigureLogging();

            this.Log().Info("========================== {0} started ==========================", this.ApplicationName);

            // setup exception handling
            this.ConfigureHandlingUnhandledExceptions();

            // configure timer (log all 10min == 600000ms)
            this.gcTimer = new Timer(this.LogMemoryUsageAndInfos, this.ApplicationStartedTime, 0, GCTimerPeriod);
        }

        public void OnExitApp(Application app)
        {
            this.gcTimer.Dispose();
            this.LogMemoryUsageAndInfos(this.ApplicationStartedTime);
            this.Log().Info("========================== {0} stopped ==========================", this.ApplicationName);
        }

        public string ApplicationName { get; private set; }

        public string ApplicationPath { get; private set; }

        public DateTime ApplicationStartedTime { get; private set; }

        public string NLogFileName { get; private set; }

        private void LogMemoryUsageAndInfos(object state)
        {
            var gcTotalMemoryInMiB = GC.GetTotalMemory(true) / 1024f / 1024f;
            var workingSetInMiB = Environment.WorkingSet / 1024f / 1024f;
            var uptime = (DateTime.UtcNow - this.ApplicationStartedTime).ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s").Replace(":", " ");
            this.Log().Info($"(GC.GetTotalMemory(true)/Environment.WorkingSet):{gcTotalMemoryInMiB:.00}/{workingSetInMiB:.00} MB" +
                            $", covers: {TinyIoCContainer.Current.Resolve<CoverManager>()}" +
                            $", started: {this.ApplicationStartedTime} (uptime: {uptime})");
        }

        private void ConfigureHandlingUnhandledExceptions()
        {
            // see more: http://dotnet.dzone.com/news/order-chaos-handling-unhandled
            Thread.GetDomain().UnhandledException += this.BackgroundThreadUnhandledException;
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.DispatcherUnhandledException += this.WPFUIThreadException;
            }
        }

        private void BackgroundThreadUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.Log().Debug("BackgroundThreadUnhandledException");
            HandleUnhandledException(e.ExceptionObject, true);
        }

        private void WPFUIThreadException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // prevent app to quit automatically, see: http://dotnet.dzone.com/news/order-chaos-handling-unhandled
            this.Log().Debug("WPFUIThreadException");
            HandleUnhandledException(e.Exception, false);
        }

        public void HandleUnhandledException(object exception, bool exitProgram)
        {
            // only allow one thread (UI)
            lock (lockErrorDialog)
            {
                try
                {
                    // check for special exitprogram conditions
                    if (!exitProgram)
                    {
                        exitProgram = ExceptionExtensions.IsFatalException((Exception)exception);
                    }
                    // prevent recursion on the message loop
                    if (!blockExceptionDialog)
                    {
                        blockExceptionDialog = true;
                        // only log once. Otherwise it is poosible to file the file system with exceptions (message pump + exception)
                        this.Log().Fatal("UnhandledException: {0}\n\n\nAt Stack: {1}", exception, Environment.StackTrace);
                        try
                        {
                            // we do not switch to the UI thread, because the dialog that we are going to show has its own message pump (all dialogs have). 
                            // As long as the dialog does not call methods of other windows there should be no problem.
                            if (exception == null)
                            {
                                //ShowStackTraceBox("Unhandled Exception Occurred", Environment.StackTrace);
                            }
                            else if (exception is Exception)
                            {
                                //ShowErrorBox((Exception)exception);
                            }
                            else
                            {
                                // won't happen really - exception is really always of type Exception
                                //ShowStackTraceBox("Unhandled Exception Occurred: " + exception, Environment.StackTrace);
                            }
                        }
                        finally
                        {
                            if (!exitProgram)
                            {
                                blockExceptionDialog = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this.Log().Fatal("Unable to Handle UnhandledException: ", e);
                }
                finally
                {
                    if (exitProgram)
                    {
                        this.Log().Warn("Application exits due to UnhandledException");
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void ConfigureLogging()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

#if DEBUG
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            consoleTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";
#endif

            // Step 3. Set target properties 
            fileTarget.CreateDirs = true;
            fileTarget.FileName = this.NLogFileName;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = true;
            fileTarget.Layout = "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=Message,Type,StackTrace:separator=//}";

            // Step 4. Define rules
#if DEBUG
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, consoleTarget));
#endif
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, fileTarget));

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            Locator.CurrentMutable.RegisterConstant(new NLogLogger(NLog.LogManager.GetCurrentClassLogger()), typeof(Splat.ILogger));
        }

        public static bool CanCreateFile(string dir)
        {
            var file = Path.Combine(dir, Guid.NewGuid().ToString() + ".tmp");
            // perhaps check File.Exists(file), but it would be a long-shot...
            bool canCreate;
            try
            {
                using (File.Create(file)) { }
                File.Delete(file);
                canCreate = true;
            }
            catch
            {
                canCreate = false;
            }
            return canCreate;
        }

        public string ApplicationDataPath()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), this.ApplicationName);
            return appData;
        }
    }
}