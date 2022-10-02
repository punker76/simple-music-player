using System;
using System.Reactive;
using System.Windows;
using System.Windows.Threading;
using FMOD;
using ReactiveUI;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.FMODStudio;
using Splat;

namespace SimpleMusicPlayer.Core.Player
{
    public static class PlayerEngineExtensions
    {
        public static PlayerEngine Configure(this PlayerEngine playerEngine)
        {
            if (playerEngine != null)
            {
                playerEngine.ConfigureInternal();
            }

            return playerEngine;
        }
    }

    public class PlayerEngine : ReactiveObject, IPlayerEngine
    {
        private FMOD.System system;
        private FMOD.Sound sound;
        private ChannelInfo channelInfo = null;
        private DispatcherTimer timer;
        private PlayerSettings playerSettings;

        public PlayerEngine(PlayerSettings settings)
        {
            this.playerSettings = settings;
        }

        internal bool ConfigureInternal()
        {
            try
            {
                this.Initializied = false;

                // Global Settings
                if (!Factory.System_Create(out this.system).ERRCHECK())
                {
                    return false;
                }

                uint version;
                this.system.getVersion(out version).ERRCHECK();
                if (version < VERSION.number)
                {
                    return false;
                }

                if (!this.system.init(16, INITFLAGS.NORMAL, (IntPtr)null).ERRCHECK())
                {
                    return false;
                }

                if (!this.system.setStreamBufferSize(128 * 1024, TIMEUNIT.RAWBYTES).ERRCHECK())
                {
                    return false;
                }

                // equalizer
                this.Equalizer = Equalizer.GetEqualizer(this.system, this.playerSettings);

                this.Volume = this.playerSettings.PlayerEngine.Volume;
                this.IsMute = this.playerSettings.PlayerEngine.Mute;
                this.State = PlayerState.Stop;
                this.LengthMs = 0;

                this.WhenAnyValue(x => x.Volume)
                    .Subscribe(newVolume =>
                    {
                        this.playerSettings.PlayerEngine.Volume = newVolume;

                        ChannelGroup masterChannelGroup;
                        this.system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();
                        masterChannelGroup.setVolume(newVolume / 100f).ERRCHECK();
                        this.system.update().ERRCHECK();
                    });

                this.WhenAnyValue(x => x.IsMute)
                    .Subscribe(mute =>
                    {
                        this.playerSettings.PlayerEngine.Mute = mute;

                        ChannelGroup masterChannelGroup;
                        this.system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();
                        masterChannelGroup.setMute(mute).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
                        this.system.update().ERRCHECK();
                    });

                var canSetCurrentPosition = this.WhenAnyValue(
                    y => y.CanSetCurrentPositionMs,
                    z => z.LengthMs,
                    (dontUpdate, length) => dontUpdate && length > 0);
                this.SetCurrentPositionMs = ReactiveCommand.Create(
                    () =>
                    {
                        this.SetCurrentPosition(this.CurrentPositionMs);
                        this.CanSetCurrentPositionMs = false;
                    },
                    canSetCurrentPosition);
                //this.SetCurrentPositionMs.IsExecuting.ToProperty(this, x => x.CanSetCurrentPositionMs, out this.canSetCurrentPositionMs);
                this.SetCurrentPositionMs.ThrownExceptions.Subscribe(ex => this.Log().ErrorException("Something went wrong", ex));

                this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(10),
                    DispatcherPriority.Normal,
                    this.PlayTimerCallback,
                    Application.Current.Dispatcher);
                this.timer.Stop();

                this.Initializied = true;

                return true;
            }
            catch (Exception exception)
            {
                this.Log().FatalException("Something went wrong", exception);
                return false;
            }
        }

        public void SetCurrentPosition(uint newPos)
        {
            var pos = newPos >= this.LengthMs ? this.LengthMs - 1 : newPos;
            this.Log().Debug($"Set new position to {pos}");
            this.channelInfo?.SetCurrentPositionMs(pos);
        }

        private void PlayTimerCallback(object sender, EventArgs e)
        {
            uint ms = 0;

            if (this.channelInfo != null && this.channelInfo.Channel.hasHandle())
            {
                var isPlaying = false;
                var isPaused = false;

                this.channelInfo.Channel.isPlaying(out isPlaying).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                this.channelInfo.Channel.getPaused(out isPaused).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                this.channelInfo.Channel.getPosition(out ms, FMOD.TIMEUNIT.MS).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);

                var completeFadeLength = this.playerSettings.PlayerEngine.FadeIn + this.playerSettings.PlayerEngine.FadeOut;
                if (completeFadeLength > 0 && isPlaying && !isPaused && LengthMs > completeFadeLength)
                {
                    var isFading = this.channelInfo.FadeVolume(0f, 1f, 0f, this.playerSettings.PlayerEngine.FadeIn, ms);
                    isFading |= this.channelInfo.FadeVolume(1f, 0f, LengthMs - this.playerSettings.PlayerEngine.FadeOut, this.playerSettings.PlayerEngine.FadeOut, ms);
                    if (!isFading)
                    {
                        this.channelInfo.Volume = 1f;
                    }
                }
            }

            if (!this.CanSetCurrentPositionMs)
            {
                this.CurrentPositionMs = ms;
            }

            //statusBar.Text = "Time " + (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + ":" + (ms / 10 % 100) + "/" + (lenms / 1000 / 60) + ":" + (lenms / 1000 % 60) + ":" + (lenms / 10 % 100) + " : " + (paused ? "Paused " : playing ? "Playing" : "Stopped");

            this.system.update();
        }

        private bool initializied;

        public bool Initializied
        {
            get => this.initializied;
            set => this.RaiseAndSetIfChanged(ref initializied, value);
        }

        private float volume = -1f;

        public float Volume
        {
            get => this.volume;
            set => this.RaiseAndSetIfChanged(ref volume, value);
        }

        private uint lengthMs;

        public uint LengthMs
        {
            get => this.lengthMs;
            set => this.RaiseAndSetIfChanged(ref lengthMs, value);
        }

        private bool canSetCurrentPositionMs;

        public bool CanSetCurrentPositionMs
        {
            get => this.canSetCurrentPositionMs;
            set => this.RaiseAndSetIfChanged(ref canSetCurrentPositionMs, value);
        }

        private uint currentPositionMs;

        public uint CurrentPositionMs
        {
            get => this.currentPositionMs;
            set => this.RaiseAndSetIfChanged(ref currentPositionMs, value);
        }

        public ReactiveCommand<Unit, Unit> SetCurrentPositionMs { get; private set; }

        private bool isMute;

        public bool IsMute
        {
            get => this.isMute;
            set => this.RaiseAndSetIfChanged(ref isMute, value);
        }

        private PlayerState state;

        public PlayerState State
        {
            get => this.state;
            set => this.RaiseAndSetIfChanged(ref state, value);
        }

        public Equalizer Equalizer { get; private set; }

        private IMediaFile currentMediaFile;

        public IMediaFile CurrentMediaFile
        {
            get => this.currentMediaFile;
            set => this.RaiseAndSetIfChanged(ref currentMediaFile, value);
        }

        public void Play(IMediaFile file)
        {
            this.CleanUpSound(ref this.sound);

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

            ChannelGroup masterChannelGroup;
            this.system.getMasterChannelGroup(out masterChannelGroup).ERRCHECK();

            if (masterChannelGroup.hasHandle() == false)
            {
                return;
            }

            // start paused for better results
            FMOD.Channel channel;
            if (!this.system.playSound(this.sound, masterChannelGroup, true, out channel).ERRCHECK())
            {
                return;
            }

            if (channel.hasHandle() == false)
            {
                return;
            }

            this.channelInfo = new ChannelInfo(channel, file, this.PlayNextFileAction);

            this.system.update().ERRCHECK();

            // now start the music
            this.timer.Start();

            this.State = PlayerState.Play;
            file.State = PlayerState.Play;

            channel.setPaused(false).ERRCHECK();
            this.system.update().ERRCHECK();
        }

        public Action PlayNextFileAction { get; set; }

        public void Pause()
        {
            if (this.channelInfo != null)
            {
                this.channelInfo.Pause();
                this.State = this.channelInfo.File.State;
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
            this.timer.Stop();

            this.State = PlayerState.Stop;
            this.CurrentMediaFile = null;

            if (this.channelInfo != null)
            {
                this.channelInfo.CleanUp();
                this.channelInfo = null;
                this.system.update().ERRCHECK();
            }

            if (fmodSound.hasHandle())
            {
                fmodSound.release().ERRCHECK();
                fmodSound.clearHandle();
                this.system.update().ERRCHECK();
            }

            this.LengthMs = 0;
            this.CurrentPositionMs = 0;
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
            if (fmodSystem.hasHandle())
            {
                fmodSystem.close().ERRCHECK();
                fmodSystem.release().ERRCHECK();
                fmodSystem.clearHandle();
            }
        }
    }
}