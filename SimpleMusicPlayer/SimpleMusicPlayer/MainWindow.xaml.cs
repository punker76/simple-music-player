using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.ViewModels;
using SimpleMusicPlayer.Base;

namespace SimpleMusicPlayer
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow
  {
    public MainWindow() {
      var vm = new MainWindowViewModel(this.Dispatcher);
      this.DataContext = vm;

      this.InitializeComponent();

      this.PreviewKeyDown += this.MainWindow_PreviewKeyDown;

      this.Title = string.Format("{0} {1}", this.Title, Assembly.GetExecutingAssembly().GetName().Version);

      this.SourceInitialized += (sender, e) => this.FitIntoScreen();

      this.Closed += (sender, e) => {
                       ((MainWindowViewModel)this.DataContext).SaveSettings();
                       PlayerEngine.Instance.CleanUp();
                     };
    }

    private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
      //bool fFocusedControlIsTextBox = FocusManager.GetFocusedElement(this).GetType().Equals(typeof(TextBox));
      var fFocusedControlIsTextBox = Keyboard.FocusedElement != null && Keyboard.FocusedElement.GetType() == typeof(TextBox);
      if (fFocusedControlIsTextBox) {
        e.Handled = false;
      } else {
        var vm = (MainWindowViewModel)this.DataContext;
        var handled = vm.HandleKeyDown(e.Key);
        e.Handled = handled;
      }
    }
  }
}