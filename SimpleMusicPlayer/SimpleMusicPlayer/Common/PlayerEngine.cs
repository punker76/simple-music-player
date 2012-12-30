using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FMOD;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class PlayerEngine : ViewModelBaseNotifyPropertyChanged, IPlayerEngine
  {
    private class ChannelInfo
    {
      public FMOD.Channel Channel { get; set; }
      public IMediaFile File { get; set; }
    }

    private FMOD.System system = null;
    private FMOD.Sound sound = null;
    private ChannelInfo channelInfo = null;
    private DispatcherTimer timer;
    private float volume;
    private TimeSpan length;
    private double currentPositionMs;
    private PlayerState state;
    private Equalizer equalizer;
    private FMOD.CHANNEL_CALLBACK channelEndCallback = new FMOD.CHANNEL_CALLBACK(ChannelEndCallback);
    private bool initializied;
    private IMediaFile currentMediaFile;
    private SMPSettings smpSettings;

    public bool Configure(Dispatcher dispatcher, SMPSettings settings) {
      this.smpSettings = settings;
      /*
          Global Settings
      */
      var result = FMOD.Factory.System_Create(ref this.system);
      if (!result.ERRCHECK()) {
        return false;
      }

      uint version = 0;
      result = this.system.getVersion(ref version);
      result.ERRCHECK();
      if (version < FMOD.VERSION.number) {
        //MessageBox.Show("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
        //Application.Exit();
        return false;
      }

      result = this.system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
      if (!result.ERRCHECK()) {
        return false;
      }

      // equalizer
      this.Equalizer = Equalizer.GetEqualizer(this.system);

      this.Volume = this.smpSettings.PlayerSettings.Volume;
      this.State = PlayerState.Stop;
      this.Length = TimeSpan.Zero;

      this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, this.PlayTimerCallback, dispatcher);
      this.timer.Stop();

      this.Initializied = true;
      return this.Initializied;
    }

    private void PlayTimerCallback(object sender, EventArgs e) {
      FMOD.RESULT result;
      uint ms = 0;
      bool playing = false;
      bool paused = false;

      if (this.channelInfo != null && this.channelInfo.Channel != null) {
        result = this.channelInfo.Channel.isPlaying(ref playing);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          result.ERRCHECK();
        }

        result = this.channelInfo.Channel.getPaused(ref paused);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          result.ERRCHECK();
        }

        result = this.channelInfo.Channel.getPosition(ref ms, FMOD.TIMEUNIT.MS);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          result.ERRCHECK();
        }
      }

      if (!this.DontUpdatePosition) {
        this.currentPositionMs = ms;
        this.OnPropertyChanged("CurrentPositionMs");
      }

      //statusBar.Text = "Time " + (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + ":" + (ms / 10 % 100) + "/" + (lenms / 1000 / 60) + ":" + (lenms / 1000 % 60) + ":" + (lenms / 10 % 100) + " : " + (paused ? "Paused " : playing ? "Playing" : "Stopped");

      if (this.system != null) {
        this.system.update();
      }
    }

    public bool Initializied {
      get { return this.initializied; }
      private set {
        if (Equals(value, this.initializied)) {
          return;
        }
        this.initializied = value;
        this.OnPropertyChanged(() => this.Initializied);
      }
    }

    public float Volume {
      get { return this.volume; }
      set {
        if (Equals(value, this.volume)) {
          return;
        }
        this.volume = value;

        this.smpSettings.PlayerSettings.Volume = value;

        if (this.channelInfo != null && this.channelInfo.Channel != null) {
          var result = this.channelInfo.Channel.setVolume(this.Volume);
          result.ERRCHECK();
        }

        this.OnPropertyChanged("Volume");
      }
    }

    public TimeSpan Length {
      get { return this.length; }
      private set {
        if (Equals(value, this.length)) {
          return;
        }
        this.length = value;
        this.OnPropertyChanged("Length");
      }
    }

    public bool DontUpdatePosition { get; set; }

    public double CurrentPositionMs {
      get { return this.currentPositionMs; }
      set {
        if (Equals(value, this.currentPositionMs)) {
          return;
        }
        this.currentPositionMs = value;

        if (this.channelInfo != null && this.channelInfo.Channel != null) {
          var result = this.channelInfo.Channel.setPosition(Convert.ToUInt32(value), FMOD.TIMEUNIT.MS);
          if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
            result.ERRCHECK();
          }
        }

        this.OnPropertyChanged("CurrentPositionMs");
      }
    }

    public PlayerState State {
      get { return this.state; }
      set {
        if (Equals(value, this.state)) {
          return;
        }
        this.state = value;
        this.OnPropertyChanged("State");
      }
    }

    public Equalizer Equalizer {
      get { return this.equalizer; }
      set {
        if (Equals(value, this.equalizer)) {
          return;
        }
        this.equalizer = value;
        this.OnPropertyChanged("Equalizer");
      }
    }

    public IMediaFile CurrentMediaFile {
      get { return this.currentMediaFile; }
      set {
        if (Equals(value, this.currentMediaFile)) {
          return;
        }
        this.currentMediaFile = value;
        this.OnPropertyChanged("CurrentMediaFile");
      }
    }

    public void Play(IMediaFile file) {
      this.CleanUpSound(ref this.sound);

      this.timer.Start();

      this.CurrentMediaFile = file;

      var mode = FMOD.MODE._2D | FMOD.MODE.CREATESTREAM;
      if (file.IsVBR) {
        mode |= FMOD.MODE.ACCURATETIME;
      }
      if (this.Equalizer != null) {
        mode |= MODE.SOFTWARE;
      } else {
        mode |= MODE.HARDWARE;
      }
      var result = this.system.createSound(file.FullFileName, mode, ref this.sound);
      if (!result.ERRCHECK()) {
        return;
      }

      uint lenms = 0;
      result = this.sound.getLength(ref lenms, FMOD.TIMEUNIT.MS);
      result.ERRCHECK();
      this.Length = TimeSpan.FromMilliseconds(lenms);

      FMOD.Channel channel = null;
      result = this.system.playSound(FMOD.CHANNELINDEX.FREE, this.sound, false, ref channel);
      if (!result.ERRCHECK()) {
        return;
      }

      this.State = PlayerState.Play;
      file.State = PlayerState.Play;

      if (channel != null) {
        this.channelInfo = new ChannelInfo() {Channel = channel, File = file};
        result = this.channelInfo.Channel.setCallback(this.channelEndCallback);
        result.ERRCHECK();

        result = this.channelInfo.Channel.setVolume(this.Volume);
        result.ERRCHECK();
      }
    }

    public Action PlayNextFileAction { get; set; }

    private static RESULT ChannelEndCallback(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, IntPtr commanddata1, IntPtr commanddata2) {
      if (type == CHANNEL_CALLBACKTYPE.END) {
        // this must be thread safe
        var currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
        var uiTask = Task.Factory.StartNew(() => {
          var action = PlayerEngine.Instance.PlayNextFileAction;
          if (action != null) {
            action();
          }
        }, CancellationToken.None, TaskCreationOptions.None, currentSynchronizationContext);
      }
      return FMOD.RESULT.OK;
    }

    public void Pause() {
      bool paused = false;
      if (this.channelInfo != null && this.channelInfo.Channel != null) {
        var result = this.channelInfo.Channel.getPaused(ref paused);
        result.ERRCHECK();

        var newPaused = !paused;
        result = this.channelInfo.Channel.setPaused(newPaused);
        result.ERRCHECK();

        this.channelInfo.File.State = newPaused ? PlayerState.Pause : PlayerState.Play;
        this.State = newPaused ? PlayerState.Pause : PlayerState.Play;
      }
    }

    public void Stop() {
      this.CleanUpSound(ref this.sound);
    }

    public void CleanUp() {
      /*
          Shut down
      */
      this.timer.Stop();
      this.CleanUpSound(ref this.sound);
      this.CleanUpEqualizer();
      this.CleanUpSystem(ref this.system);
    }

    private void CleanUpSound(ref FMOD.Sound fmodSound) {
      this.State = PlayerState.Stop;
      this.CurrentMediaFile = null;

      if (this.channelInfo != null && this.channelInfo.Channel != null) {
        this.channelInfo.File.State = PlayerState.Stop;
        this.channelInfo.Channel.setCallback(null);
        this.channelInfo.Channel = null;
        this.channelInfo.File = null;
        this.channelInfo = null;
      }

      if (fmodSound != null) {
        var result = fmodSound.release();
        result.ERRCHECK();
        fmodSound = null;
      }

      this.timer.Stop();

      this.currentPositionMs = 0;
      this.OnPropertyChanged("CurrentPositionMs");
      this.Length = TimeSpan.Zero;
    }

    private void CleanUpEqualizer() {
      if (this.Equalizer != null) {
        this.Equalizer.CleanUp();
        this.Equalizer = null;
      }
    }

    private void CleanUpSystem(ref FMOD.System fmodSystem) {
      if (fmodSystem != null) {
        var result = fmodSystem.close();
        result.ERRCHECK();
        result = fmodSystem.release();
        result.ERRCHECK();
        fmodSystem = null;
      }
    }

    private static PlayerEngine instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static PlayerEngine() {
    }

    private PlayerEngine() {
    }

    public static PlayerEngine Instance {
      get { return instance ?? (instance = new PlayerEngine()); }
    }
  }

  public static class PlayerEngineExtensions
  {
    public static bool ERRCHECK(this FMOD.RESULT result) {
      if (result != FMOD.RESULT.OK) {
        //this.timer.Stop();
        MessageBox.Show("FMOD error! " + result + " - " + FMOD.Error.String(result));
        // todo : show error info dialog
        //Environment.Exit(-1);
        return false;
      }
      return true;
    }
  }
}