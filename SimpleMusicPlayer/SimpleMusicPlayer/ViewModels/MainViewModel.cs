using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Common.Extensions;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.Views;

namespace SimpleMusicPlayer.ViewModels
{
  public class MainViewModel : ViewModelBase, IKeyHandler
  {
    private EqualizerViewModel equalizerViewModel;
    private ICommand showOnGitHubCmd;
    private ICommand showEqualizerCommand;
    private ICommand closeEqualizerCommand;
    private MedialibView medialibView;

    public MainViewModel(Dispatcher dispatcher) {
      this.PlayerSettings = PlayerSettingsExtensions.ReadSettings();
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.PlayerSettings.MainWindow);
      
      this.PlayerEngine.Configure(dispatcher, this.PlayerSettings);

      this.PlayListFileSearchWorker = new FileSearchWorker();
      this.MedialibFileSearchWorker = new FileSearchWorker();
      
      this.MedialibViewModel = new MedialibViewModel(dispatcher, this);
      this.PlayListsViewModel = new PlayListsViewModel(dispatcher, this);
      
      this.PlayControlInfoViewModel = new PlayControlInfoViewModel(dispatcher, this);
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings { get; private set; }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public PlayerSettings PlayerSettings { get; private set; }

    public void SaveSettings() {
      this.PlayerSettings.WriteSettings();
    }

    public FileSearchWorker PlayListFileSearchWorker { get; private set; }

    public FileSearchWorker MedialibFileSearchWorker { get; private set; }

    public PlayControlInfoViewModel PlayControlInfoViewModel { get; private set; }

    public PlayListsViewModel PlayListsViewModel { get; private set; }

    public MedialibViewModel MedialibViewModel { get; private set; }

    public void ShowMediaLibrary() {
      if (this.medialibView != null) {
        this.medialibView.Activate();
      } else {
        this.medialibView = new MedialibView(this.MedialibViewModel);
        this.medialibView.Closed += (sender, args) => this.medialibView = null;
        this.medialibView.Show();
      }
    }

    public EqualizerViewModel EqualizerViewModel {
      get { return this.equalizerViewModel; }
      set {
        if (Equals(value, this.equalizerViewModel)) {
          return;
        }
        this.equalizerViewModel = value;
        this.OnPropertyChanged(() => this.EqualizerViewModel);
      }
    }

    public ICommand ShowOnGitHubCmd {
      get { return this.showOnGitHubCmd ?? (this.showOnGitHubCmd = new DelegateCommand(this.ShowOnGitHub, () => true)); }
    }

    private void ShowOnGitHub() {
      System.Diagnostics.Process.Start("https://github.com/punker76/simple-music-player");
    }

    public ICommand ShowEqualizerCommand {
      get { return this.showEqualizerCommand ?? (this.showEqualizerCommand = new DelegateCommand(this.ShowEqualizer, this.CanShowEqualizer)); }
    }

    private bool CanShowEqualizer() {
      return this.PlayerEngine.Initializied;
    }

    private void ShowEqualizer() {
      this.EqualizerViewModel = new EqualizerViewModel(this.PlayerEngine.Equalizer);
    }

    public ICommand CloseEqualizerCommand {
      get { return this.closeEqualizerCommand ?? (this.closeEqualizerCommand = new DelegateCommand(this.CloseEqualizer, this.CanCloseEqualizer)); }
    }

    private bool CanCloseEqualizer() {
      return true;
    }

    private void CloseEqualizer() {
      if (this.EqualizerViewModel.Equalizer.IsEnabled) {
        this.EqualizerViewModel.Equalizer.SaveEqualizerSettings();
      }
      this.EqualizerViewModel.Equalizer = null;
      this.EqualizerViewModel = null;
    }

    public bool HandleKeyDown(Key key) {
      if (this.PlayControlInfoViewModel.PlayControlViewModel.HandleKeyDown(key)) {
        return true;
      }
      if (key == Key.E && this.ShowEqualizerCommand.CanExecute(null)) {
        this.ShowEqualizerCommand.Execute(null);
        return true;
      }
      return false;
    }
  }
}