using System;
using System.Reflection;
using Microsoft.Shell;

namespace SimpleMusicPlayer
{
    public class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var resourceName = Assembly.GetExecutingAssembly().GetName().Name + ".DllsAsResource." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            };
        }

        [STAThread]
        public static void Main()
        {
            StartUp();
        }

        private static void StartUp()
        {
            if (SingleInstance<SimpleMusicPlayerApplication.App>.InitializeAsFirstInstance("18980929-1342-4467-bc3d-37b0d13fa938"))
            {
                var application = new SimpleMusicPlayerApplication.App();
                application.Init();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<SimpleMusicPlayerApplication.App>.Cleanup();
            }
        }
    }
}