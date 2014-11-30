using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FMOD;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core
{
    public class PlayerEngine : ViewModelBase, IPlayerEngine
    {
        private class ChannelInfo
        {
            public FMOD.Channel Channel { get; set; }
            public IMediaFile File { get; set; }
            public DSP FaderDSP { get; set; }

            public void CleanUp()
            {
                if (this.Channel != null)
                {
                    this.Channel.setVolume(0f).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
                    if (this.FaderDSP != null)
                    {
                        this.Channel.removeDSP(this.FaderDSP).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
                    }
                    this.Channel.setCallback(null).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
                    this.Channel = null;
                }

                this.File.State = PlayerState.Stop;
                this.File = null;
            }
        }

        private FMOD.System system = null;
        private FMOD.Sound sound = null;
        private ChannelInfo channelInfo = null;
        private DispatcherTimer timer;
        private float volume;
        private uint lengthMs;
        private uint currentPositionMs;
        private bool isMute;
        private PlayerState state;
        private Equalizer equalizer;
        private readonly FMOD.CHANNEL_CALLBACK channelEndCallback = new FMOD.CHANNEL_CALLBACK(ChannelEndCallback);
        private bool initializied;
        private IMediaFile currentMediaFile;
        private PlayerSettings playerSettings;

        public bool Configure(Dispatcher dispatcher, PlayerSettings settings)
        {
            this.playerSettings = settings;
            /*
                Global Settings
            */
            if (!FMOD.Factory.System_Create(out this.system).ERRCHECK())
            {
                return false;
            }

            uint version;
            this.system.getVersion(out version).ERRCHECK();
            if (version < FMOD.VERSION.number)
            {
                //MessageBox.Show("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
                //Application.Exit();
                return false;
            }

            if (!this.system.init(16, FMOD.INITFLAGS.NORMAL, (IntPtr)null).ERRCHECK())
            {
                return false;
            }

            if (!this.system.setStreamBufferSize(64 * 1024, FMOD.TIMEUNIT.RAWBYTES).ERRCHECK())
            {
                return false;
            }

            // equalizer
            this.Equalizer = Equalizer.GetEqualizer(this.system, settings);

            this.Volume = this.playerSettings.PlayerEngine.Volume;
            this.IsMute = this.playerSettings.PlayerEngine.Mute;
            this.State = PlayerState.Stop;
            this.LengthMs = 0;

            this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, this.PlayTimerCallback, dispatcher);
            this.timer.Stop();

            this.Initializied = true;
            return this.Initializied;
        }

        private void PlayTimerCallback(object sender, EventArgs e)
        {
            uint ms = 0;
            var playing = false;
            var paused = false;

            if (this.channelInfo != null && this.channelInfo.Channel != null)
            {
                this.channelInfo.Channel.isPlaying(out playing).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                this.channelInfo.Channel.getPaused(out paused).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                this.channelInfo.Channel.getPosition(out ms, FMOD.TIMEUNIT.MS).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
            }

            if (!this.DontUpdatePosition)
            {
                this.currentPositionMs = ms;
                this.OnPropertyChanged("CurrentPositionMs");
            }

            //statusBar.Text = "Time " + (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + ":" + (ms / 10 % 100) + "/" + (lenms / 1000 / 60) + ":" + (lenms / 1000 % 60) + ":" + (lenms / 10 % 100) + " : " + (paused ? "Paused " : playing ? "Playing" : "Stopped");

            if (this.system != null)
            {
                this.system.update();
            }
        }

        public bool Initializied
        {
            get { return this.initializied; }
            private set
            {
                if (Equals(value, this.initializied))
                {
                    return;
                }
                this.initializied = value;
                this.OnPropertyChanged(() => this.Initializied);
            }
        }

        public float Volume
        {
            get { return this.volume; }
            set
            {
                if (Equals(value, this.volume))
                {
                    return;
                }
                this.volume = value;
                this.playerSettings.PlayerEngine.Volume = value;

                ChannelGroup masterChannelGroup;
                this.system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();
                masterChannelGroup.setVolume(value / 100f).ERRCHECK();

                this.system.update().ERRCHECK();

                this.OnPropertyChanged("Volume");
            }
        }

        public uint LengthMs
        {
            get { return this.lengthMs; }
            private set
            {
                if (Equals(value, this.lengthMs))
                {
                    return;
                }
                this.lengthMs = value;
                this.OnPropertyChanged("LengthMs");
            }
        }

        public bool DontUpdatePosition { get; set; }

        public uint CurrentPositionMs
        {
            get { return this.currentPositionMs; }
            set
            {
                if (Equals(value, this.currentPositionMs))
                {
                    return;
                }

                this.currentPositionMs = value >= this.LengthMs ? this.LengthMs - 1 : value;

                if (this.channelInfo != null && this.channelInfo.Channel != null)
                {
                    this.channelInfo.Channel.setPosition(this.currentPositionMs, FMOD.TIMEUNIT.MS).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                    this.system.update().ERRCHECK();
                }

                this.OnPropertyChanged("CurrentPositionMs");
            }
        }

        public bool IsMute
        {
            get { return this.isMute; }
            set
            {
                if (Equals(value, this.isMute))
                {
                    return;
                }
                this.isMute = value;
                this.playerSettings.PlayerEngine.Mute = value;

                if (this.channelInfo != null && this.channelInfo.Channel != null)
                {
                    this.channelInfo.Channel.setMute(value).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                    this.system.update().ERRCHECK();
                }

                this.OnPropertyChanged("IsMute");
            }
        }

        public PlayerState State
        {
            get { return this.state; }
            set
            {
                if (Equals(value, this.state))
                {
                    return;
                }
                this.state = value;
                this.OnPropertyChanged("State");
            }
        }

        public Equalizer Equalizer
        {
            get { return this.equalizer; }
            set
            {
                if (Equals(value, this.equalizer))
                {
                    return;
                }
                this.equalizer = value;
                this.OnPropertyChanged("Equalizer");
            }
        }

        public IMediaFile CurrentMediaFile
        {
            get { return this.currentMediaFile; }
            set
            {
                if (Equals(value, this.currentMediaFile))
                {
                    return;
                }
                this.currentMediaFile = value;
                this.OnPropertyChanged("CurrentMediaFile");
            }
        }

        public void Play(IMediaFile file)
        {
            this.CleanUpSound(ref this.sound);

            this.timer.Start();

            this.CurrentMediaFile = file;

            var mode = FMOD.MODE.DEFAULT | FMOD.MODE._2D | FMOD.MODE.CREATESTREAM | FMOD.MODE.LOOP_OFF;
            if (file.IsVBR)
            {
                mode |= FMOD.MODE.ACCURATETIME;
            }

            if (!this.system.createSound(file.FullFileName, mode, out this.sound).ERRCHECK())
            {
                return;
            }

            uint lenms;
            this.sound.getLength(out lenms, FMOD.TIMEUNIT.MS).ERRCHECK();
            this.LengthMs = lenms;

            uint length;
            this.sound.getLength(out length, FMOD.TIMEUNIT.PCM).ERRCHECK();

            FMOD.Channel channel = null;
            if (!this.system.playSound(this.sound, null, false, out channel).ERRCHECK())
            {
                return;
            }

            this.State = PlayerState.Play;
            file.State = PlayerState.Play;

            if (channel != null)
            {
                this.channelInfo = new ChannelInfo() { Channel = channel, File = file };

                channel.setCallback(this.channelEndCallback).ERRCHECK();

                channel.setVolume(1f).ERRCHECK();

                ChannelGroup masterChannelGroup;
                this.system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();
                masterChannelGroup.setVolume(this.Volume / 100f).ERRCHECK();

                this.system.update().ERRCHECK();

                FMOD.DSP faderDSP;
                this.system.createDSPByType(FMOD.DSP_TYPE.FADER, out faderDSP).ERRCHECK();

                this.channelInfo.FaderDSP = faderDSP;

                int numDSPs;
                channel.getNumDSPs(out numDSPs).ERRCHECK();

                channel.addDSP(numDSPs, faderDSP).ERRCHECK();

                ulong dspclock;
                ulong parentclock;
                channel.getDSPClock(out dspclock, out parentclock).ERRCHECK();

                int samplerate;
                SPEAKERMODE speakermode;
                int numrawspeakers;
                this.system.getSoftwareFormat(out samplerate, out speakermode, out numrawspeakers).ERRCHECK();

                var samples = (uint)Math.Round(5000f * samplerate / 1000f);

                channel.addFadePoint(parentclock, 0f).ERRCHECK();
                channel.addFadePoint(parentclock + samples, 1f).ERRCHECK();

                //channel.addFadePoint(parentclock + length - samples, 1f).ERRCHECK();
                //channel.addFadePoint(parentclock + length, 0f).ERRCHECK();

                this.system.update().ERRCHECK();
            }
        }

        public Action PlayNextFileAction { get; set; }

        private static RESULT ChannelEndCallback(IntPtr channelraw, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (type == CHANNELCONTROL_CALLBACK_TYPE.END)
            {
                // this must be thread safe
                var currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
                var uiTask = Task.Factory.StartNew(() => {
                    var action = PlayerEngine.Instance.PlayNextFileAction;
                    if (action != null)
                    {
                        action();
                    }
                }, CancellationToken.None, TaskCreationOptions.None, currentSynchronizationContext);
            }
            return FMOD.RESULT.OK;
        }

        public void Pause()
        {
            if (this.channelInfo != null && this.channelInfo.Channel != null)
            {
                bool paused;
                this.channelInfo.Channel.getPaused(out paused).ERRCHECK();

                var newPaused = !paused;
                this.channelInfo.Channel.setPaused(newPaused).ERRCHECK();

                this.system.update().ERRCHECK();

                this.channelInfo.File.State = newPaused ? PlayerState.Pause : PlayerState.Play;
                this.State = newPaused ? PlayerState.Pause : PlayerState.Play;
            }
        }

        public void Stop()
        {
            this.CleanUpSound(ref this.sound);
        }

        public void CleanUp()
        {
            /*
                Shut down
            */
            this.timer.Stop();
            this.CleanUpSound(ref this.sound);
            this.CleanUpEqualizer();
            this.CleanUpSystem(ref this.system);
            this.playerSettings = null;
        }

        private void CleanUpSound(ref FMOD.Sound fmodSound)
        {
            this.State = PlayerState.Stop;
            this.CurrentMediaFile = null;

            if (this.channelInfo != null)
            {
                this.channelInfo.CleanUp();
                this.channelInfo = null;
            }

            if (fmodSound != null)
            {
                fmodSound.release().ERRCHECK();
                fmodSound = null;
            }

            this.timer.Stop();

            this.currentPositionMs = 0;
            this.OnPropertyChanged("CurrentPositionMs");
            this.LengthMs = 0;
        }

        private void CleanUpEqualizer()
        {
            if (this.Equalizer != null)
            {
                this.Equalizer.CleanUp();
                this.Equalizer = null;
            }
        }

        private void CleanUpSystem(ref FMOD.System fmodSystem)
        {
            if (fmodSystem != null)
            {
                fmodSystem.close().ERRCHECK();
                fmodSystem.release().ERRCHECK();
                fmodSystem = null;
            }
        }

        private static PlayerEngine instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static PlayerEngine()
        {
        }

        private PlayerEngine()
        {
        }

        public static PlayerEngine Instance
        {
            get { return instance ?? (instance = new PlayerEngine()); }
        }
    }
}