using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ViewModels
{
  public class MainWindowViewModel : ViewModelBaseNotifyPropertyChanged
  {
    private readonly SMPSettings smpSettings;
    private PlayControlViewModel playControlViewModel;
    private PlayInfoViewModel playInfoViewModel;
    private PlaylistsViewModel playlistsViewModel;
    private MedialibViewModel medialibViewModel;
    private EqualizerViewModel equalizerViewModel;
    private ICommand showOnGitHubCmd;
    private ICommand showEqualizerCommand;
    private ICommand closeEqualizerCommand;

    public MainWindowViewModel(Dispatcher dispatcher, InputBindingCollection inputBindings) {
      this.smpSettings = this.ReadSettings();
      this.PlayerEngine.Configure(dispatcher, this.smpSettings);
      this.PlaylistsViewModel = new PlaylistsViewModel(dispatcher, this.smpSettings);
      this.PlayControlViewModel = new PlayControlViewModel(dispatcher, this.smpSettings, this.PlaylistsViewModel);
      this.PlayInfoViewModel = new PlayInfoViewModel(dispatcher);
      this.MedialibViewModel = new MedialibViewModel(dispatcher);

      //inputBindings.Add(new KeyBinding(this.PlayControlViewModel.RepeatCommand, Key.R, ModifierKeys.None));
      inputBindings.Add(new KeyBinding() {Command = this.PlayControlViewModel.RepeatCommand, Key = Key.R});
      inputBindings.Add(new KeyBinding() {Command = this.PlayControlViewModel.ShuffleCommand, Key = Key.S});
      inputBindings.Add(new KeyBinding() {Command = this.PlayControlViewModel.PlayNextCommand, Key = Key.J});
      inputBindings.Add(new KeyBinding() {Command = this.PlayControlViewModel.PlayPrevCommand, Key = Key.K});
      inputBindings.Add(new KeyBinding() {Command = this.PlayControlViewModel.PlayOrPauseCommand, Key = Key.Space});
      inputBindings.Add(new KeyBinding() {Command = this.PlaylistsViewModel.PlayCommand, Key = Key.Enter});
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    private SMPSettings ReadSettings() {
      if (File.Exists(SMPSettings.SettingsFile)) {
        var jsonString = File.ReadAllText(SMPSettings.SettingsFile);
        return JsonConvert.DeserializeObject<SMPSettings>(jsonString);
      }
      return SMPSettings.GetEmptySettings();
    }

    private void WriteSettings(SMPSettings settings) {
      var settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
      File.WriteAllText(SMPSettings.SettingsFile, settingsAsJson);
    }

    public void SaveSettings() {
      this.WriteSettings(this.smpSettings);
    }

    public PlayControlViewModel PlayControlViewModel {
      get { return this.playControlViewModel; }
      set {
        if (Equals(value, this.playControlViewModel)) {
          return;
        }
        this.playControlViewModel = value;
        this.OnPropertyChanged(() => this.PlayControlViewModel);
      }
    }

    public PlayInfoViewModel PlayInfoViewModel {
      get { return this.playInfoViewModel; }
      set {
        if (Equals(value, this.playInfoViewModel)) {
          return;
        }
        this.playInfoViewModel = value;
        this.OnPropertyChanged(() => this.PlayInfoViewModel);
      }
    }

    public PlaylistsViewModel PlaylistsViewModel {
      get { return this.playlistsViewModel; }
      set {
        if (Equals(value, this.playlistsViewModel)) {
          return;
        }
        this.playlistsViewModel = value;
        this.OnPropertyChanged(() => this.PlaylistsViewModel);
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
  }
}