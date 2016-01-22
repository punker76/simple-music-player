using System;
using System.Runtime.Remoting;
using Microsoft.Shell;

namespace SimpleMusicPlayer
{
    public class Program
    {
        private const int MAXTRIES = 10;

        [STAThread]
        public static void Main()
        {
            StartUp();
        }

        private static void StartUp()
        {
            var isFirstInstance = false;

            for (var i = 1; i <= MAXTRIES; i++)
            {
                try
                {
                    isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance("18980929-1342-4467-bc3d-37b0d13fa938");
                    break;
                }
                catch (RemotingException)
                {
                    break;
                }
                catch (Exception)
                {
                    if (i == MAXTRIES)
                    {
                        return;
                    }
                }
            }

            if (isFirstInstance)
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
    }
}