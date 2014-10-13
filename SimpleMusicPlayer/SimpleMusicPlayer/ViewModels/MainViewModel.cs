using System.Windows.Input;
using System.Windows.Threading;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Common.Extensions;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class MainViewModel : ViewModelBase, IKeyHandler
  {
    private PlayControlInfoViewModel playControlInfoViewModel;
    private PlayListsViewModel playListsViewModel;
    private MedialibViewModel medialibViewModel;
    private EqualizerViewModel equalizerViewModel;
    private ICommand showOnGitHubCmd;
    private ICommand showEqualizerCommand;
    private ICommand closeEqualizerCommand;
    private CustomWindowPlacementSettings customWindowPlacementSettings;

    public MainViewModel(Dispatcher dispatcher) {
      this.PlayerSettings = PlayerSettingsExtensions.ReadSettings();
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.PlayerSettings.MainWindow);
      this.PlayerEngine.Configure(dispatcher, this.PlayerSettings);
      this.MedialibViewModel = new MedialibViewModel(dispatcher, this.PlayerSettings);
      this.PlayListsViewModel = new PlayListsViewModel(dispatcher, this.PlayerSettings);
      this.PlayControlInfoViewModel = new PlayControlInfoViewModel(dispatcher) {
        PlayControlViewModel = new PlayControlViewModel(dispatcher, this.PlayerSettings, this.PlayListsViewModel, this.MedialibViewModel),
        PlayInfoViewModel = new PlayInfoViewModel(dispatcher)
      };
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings {
      get { return customWindowPlacementSettings; }
      set {
        if (Equals(value, this.customWindowPlacementSettings)) {
          return;
        }
        this.customWindowPlacementSettings = value;
        this.OnPropertyChanged(() => this.CustomWindowPlacementSettings);
      }
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public PlayerSettings PlayerSettings { get; private set; }

    public void SaveSettings() {
      this.PlayerSettings.WriteSettings();
    }

    public PlayControlInfoViewModel PlayControlInfoViewModel {
      get { return this.playControlInfoViewModel; }
      set {
        if (Equals(value, this.playControlInfoViewModel)) {
          return;
        }
        this.playControlInfoViewModel = value;
        this.OnPropertyChanged(() => this.PlayControlInfoViewModel);
      }
    }

    public PlayListsViewModel PlayListsViewModel {
      get { return this.playListsViewModel; }
      set {
        if (Equals(value, this.playListsViewModel)) {
          return;
        }
        this.playListsViewModel = value;
        this.OnPropertyChanged(() => this.PlayListsViewModel);
      }
    }

    public MedialibViewModel MedialibViewModel {
      get { return this.medialibViewModel; }
      set {
        if (Equals(value, this.medialibViewModel)) {
          return;
        }
        this.medialibViewModel = value;
        this.OnPropertyChanged(() => this.MedialibViewModel);
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