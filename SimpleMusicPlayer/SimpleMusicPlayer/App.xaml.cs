using System;
using System.Reflection;
using System.Windows;

namespace SimpleMusicPlayer
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e) {
      AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
        String resourceName = "SimpleMusicPlayer.DllsAsResource." + new AssemblyName(args.Name).Name + ".dll";
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
          if (stream != null) {
            Byte[] assemblyData = new Byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            return Assembly.Load(assemblyData);
          }
        }
        return null;
      };
      base.OnStartup(e);
    }
  }
}