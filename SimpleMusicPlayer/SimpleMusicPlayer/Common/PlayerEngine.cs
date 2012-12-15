using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FMOD;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public class PlayerEngine : ViewModelBaseNotifyPropertyChanged, IPlayerEngine
  {
    private FMOD.System system = null;
    private FMOD.Sound sound = null;
    private FMOD.Channel channel = null;
    private DispatcherTimer timer;
    private float volume;
    private TimeSpan length;
    private double currentPositionMs;

    public bool Configure(Dispatcher dispatcher) {
      /*
          Global Settings
      */
      var result = FMOD.Factory.System_Create(ref this.system);
      this.ERRCHECK(result);

      uint version = 0;
      result = this.system.getVersion(ref version);
      this.ERRCHECK(result);
      if (version < FMOD.VERSION.number) {
        //MessageBox.Show("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
        //Application.Exit();
        return false;
      }

      result = this.system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
      this.ERRCHECK(result);

      result = this.system.attachFileSystem(this.myopen, this.myclose, null, null);
      this.ERRCHECK(result);

      this.Volume = 1;

      this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, this.PlayTimerCallback, dispatcher);

      return true;
    }

    private FMOD.FILE_OPENCALLBACK myopen = new FMOD.FILE_OPENCALLBACK(OPENCALLBACK);
    private FMOD.FILE_CLOSECALLBACK myclose = new FMOD.FILE_CLOSECALLBACK(CLOSECALLBACK);

    private static FMOD.RESULT OPENCALLBACK([MarshalAs(UnmanagedType.LPWStr)] string name, int unicode, ref uint filesize, ref IntPtr handle, ref IntPtr userdata) {
      return FMOD.RESULT.OK;
    }

    private static FMOD.RESULT CLOSECALLBACK(IntPtr handle, IntPtr userdata) {
      return FMOD.RESULT.OK;
    }

    private void PlayTimerCallback(object sender, EventArgs e) {
      FMOD.RESULT result;
      uint ms = 0;
      bool playing = false;
      bool paused = false;

      if (this.channel != null) {
        result = this.channel.isPlaying(ref playing);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          this.ERRCHECK(result);
        }

        result = this.channel.getPaused(ref paused);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          this.ERRCHECK(result);
        }

        result = this.channel.getPosition(ref ms, FMOD.TIMEUNIT.MS);
        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
          this.ERRCHECK(result);
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

    public float Volume {
      get { return this.volume; }
      set {
        if (Equals(value, this.volume)) {
          return;
        }
        this.volume = value;

        if (this.channel != null) {
          var result = this.channel.setVolume(this.Volume);
          this.ERRCHECK(result);
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

        if (this.channel != null) {
          var result = this.channel.setPosition(Convert.ToUInt32(value), FMOD.TIMEUNIT.MS);
          if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE)) {
            this.ERRCHECK(result);
          }
        }

        this.OnPropertyChanged("CurrentPositionMs");
      }
    }

    public void Play(IMediaFile file) {
      this.CleanUpSound(this.sound);

      var result = this.system.createSound(file.FullFileName, (FMOD.MODE._2D | FMOD.MODE.HARDWARE | FMOD.MODE.CREATESTREAM), ref this.sound);
      this.ERRCHECK(result);

      uint lenms = 0;
      result = this.sound.getLength(ref lenms, FMOD.TIMEUNIT.MS);
      this.ERRCHECK(result);
      this.Length = TimeSpan.FromMilliseconds(lenms);

      result = this.system.playSound(FMOD.CHANNELINDEX.FREE, this.sound, false, ref this.channel);
      this.ERRCHECK(result);

      if (this.channel != null) {
        this.channel.setCallback(this.channelEndCallback);

        result = this.channel.setVolume(this.Volume);
        this.ERRCHECK(result);
      }
    }

    public Action PlayNextFileAction { get; set; }

    private FMOD.CHANNEL_CALLBACK channelEndCallback = new FMOD.CHANNEL_CALLBACK(ChannelEndCallback);

    private static RESULT ChannelEndCallback(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, IntPtr commanddata1, IntPtr commanddata2) {
      if (type == CHANNEL_CALLBACKTYPE.END) {
        // this must be thread safe
        var uiTask = Task.Factory.StartNew(() => {
          var action = PlayerEngine.Instance.PlayNextFileAction;
          if (action != null) {
            action();
          }
        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
      }
      return FMOD.RESULT.OK;
    }

    public void Pause() {
      bool paused = false;
      if (this.channel != null) {
        var result = this.channel.getPaused(ref paused);
        this.ERRCHECK(result);
        result = this.channel.setPaused(!paused);
        this.ERRCHECK(result);
      }
    }

    public void CleanUp() {
      this.timer.Stop();
      /*
          Shut down
      */
      this.CleanUpSound(this.sound);
      this.CleanUpSystem(this.system);
    }

    private void CleanUpSound(FMOD.Sound fmodSound) {
      if (this.channel != null) {
        this.channel.setCallback(null);
      }
      if (fmodSound != null) {
        var result = fmodSound.release();
        this.ERRCHECK(result);
      }
    }

    private void CleanUpSystem(FMOD.System fmodSystem) {
      if (fmodSystem != null) {
        var result = fmodSystem.close();
        this.ERRCHECK(result);
        result = fmodSystem.release();
        this.ERRCHECK(result);
      }
    }

    private void ERRCHECK(FMOD.RESULT result) {
      if (result != FMOD.RESULT.OK) {
        this.timer.Stop();
        //MessageBox.Show("FMOD error! " + result + " - " + FMOD.Error.String(result));
        Environment.Exit(-1);
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
}